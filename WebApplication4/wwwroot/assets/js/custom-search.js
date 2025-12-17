(function () {
    'use strict';

    /**
     * custom-search.js
     * - If #products-grid exists (Shop page) perform client-side filtering (like Wishlist).
     * - Otherwise do debounced AJAX to /Shop/SearchProducts.
     * - Defensive: handles non-JSON responses, shows friendly messages, logs useful debug info.
     */

    document.addEventListener('DOMContentLoaded', function () {
        // Elements (must match markup in Header component)
        const searchPopup = document.querySelector('.search-popup');
        const searchInput = document.getElementById('headerSearchInput');
        const searchResults = document.getElementById('searchResults');
        const searchLoading = document.getElementById('searchLoading');
        const searchNoResults = document.getElementById('searchNoResults');
        const openSearchBtns = Array.from(document.querySelectorAll('.js-search-popup'));
        const closeSearchBtn = document.querySelector('.js-close-search');

        if (!searchPopup || !searchInput) {
            console.warn('custom-search: required elements are missing; aborting initialization.');
            return;
        }

        // Detect local products grid (Shop page) for client-side search
        const productsGrid = document.getElementById('products-grid');
        let clientSideCards = productsGrid ? Array.from(productsGrid.querySelectorAll('.product-card-wrapper')) : null;

        // UI helpers
        const DEBOUNCE_MS = 300;
        let debounceTimer = null;

        function showLoading() { if (searchLoading) searchLoading.classList.remove('d-none'); }
        function hideLoading() { if (searchLoading) searchLoading.classList.add('d-none'); }
        function showNoResults() { if (searchNoResults) searchNoResults.classList.remove('d-none'); }
        function hideNoResults() { if (searchNoResults) searchNoResults.classList.add('d-none'); }
        function clearResults() { if (searchResults) searchResults.innerHTML = ''; }

        // Popup open/close
        function openSearch() {
            searchPopup.style.display = 'block';
            document.body.style.overflow = 'hidden';
            setTimeout(() => { try { searchInput.focus(); } catch (e) { } }, 80);
        }
        function closeSearch() {
            searchPopup.style.display = 'none';
            document.body.style.overflow = '';
            searchInput.value = '';
            clearResults();
            hideLoading();
            hideNoResults();
            // If client-side, reset product-grid visibility back to normal
            if (clientSideCards) {
                clientSideCards.forEach(c => {
                    c.style.display = '';
                    c.classList.remove('d-none');
                });
                if (productsGrid) {
                    const noResultsEl = document.getElementById('wishlistNoResults');
                    if (noResultsEl) noResultsEl.classList.add('d-none');
                }
            }
        }

        openSearchBtns.forEach(btn => btn.addEventListener('click', e => { e.preventDefault(); e.stopPropagation(); openSearch(); }));
        if (closeSearchBtn) closeSearchBtn.addEventListener('click', e => { e.preventDefault(); closeSearch(); });
        searchPopup.addEventListener('click', e => { if (e.target === searchPopup) closeSearch(); });
        document.addEventListener('keydown', e => { if (e.key === 'Escape' && searchPopup.style.display === 'block') closeSearch(); });

        // Helpers for rendering server results (unchanged)
        function escapeHtml(str) {
            if (!str) return '';
            return String(str).replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;')
                .replace(/"/g, '&quot;').replace(/'/g, '&#039;');
        }

        function buildResultItem(product) {
            const id = product.id || '';
            const name = product.name || 'Unknown product';
            const image = product.coverImageName || product.image || 'product-placeholder.jpg';
            const category = product.categoryName || product.category || '';
            const priceVal = (product.basePrice !== undefined) ? product.basePrice : (product.price || product.salePrice || 0);
            const price = (typeof priceVal === 'number') ? priceVal.toFixed(2) : parseFloat(priceVal || 0).toFixed(2);
            const detailsUrl = product.detailsUrl || product.url || product.slug || id;

            const a = document.createElement('a');
            a.href = `/Product/Details/${encodeURIComponent(detailsUrl)}`;
            a.className = 'list-group-item list-group-item-action border-0 py-3 search-result-item';
            a.innerHTML = `
                <div class="d-flex align-items-center gap-3">
                    <img src="/images/products/${encodeURIComponent(image)}" alt="${escapeHtml(name)}"
                         style="width:70px;height:70px;object-fit:cover;border-radius:8px;flex-shrink:0;"
                         onerror="this.onerror=null;this.src='/assets/images/no-image.png';" />
                    <div class="flex-grow-1" style="min-width:0;">
                        <h6 class="mb-1 text-truncate" title="${escapeHtml(name)}">${escapeHtml(name)}</h6>
                        <p class="mb-0 text-muted small text-truncate" title="${escapeHtml(category)}">${escapeHtml(category)}</p>
                    </div>
                    <div class="text-end" style="flex-shrink:0;">
                        <strong class="text-primary fs-6">$${price}</strong>
                    </div>
                </div>
            `;
            return a;
        }

        function renderResults(products) {
            clearResults();
            hideNoResults();
            if (!Array.isArray(products) || products.length === 0) {
                showNoResults();
                return;
            }
            const wrapper = document.createElement('div');
            wrapper.className = 'list-group list-group-flush';
            products.forEach(p => {
                try { wrapper.appendChild(buildResultItem(p)); } catch (err) { console.warn('custom-search render error', err, p); }
            });
            if (searchResults) searchResults.appendChild(wrapper);
        }

        // CLIENT-SIDE FILTER (Shop page) - operate directly on DOM product cards
        function clientSideFilter(term) {
            if (!clientSideCards) return;
            const q = String(term || '').trim().toLowerCase();
            let visible = 0;

            // If empty, reset to normal infinite-scroll visibility (remove inline display/none)
            if (q.length === 0) {
                clientSideCards.forEach(c => {
                    c.style.display = '';
                    c.classList.remove('d-none');
                });
                const wishlistNoResults = document.getElementById('wishlistNoResults');
                if (wishlistNoResults) wishlistNoResults.classList.add('d-none');
                return;
            }

            clientSideCards.forEach(card => {
                const title = (card.querySelector('.pc__title')?.textContent || '').trim().toLowerCase();
                const category = (card.querySelector('.pc__category')?.textContent || '').trim().toLowerCase();
                const match = title.includes(q) || category.includes(q);
                card.style.display = match ? '' : 'none';
                card.classList.remove('d-none'); // ensure visible ones aren't hidden via d-none
                if (match) visible++;
            });

            const wishlistNoResults = document.getElementById('wishlistNoResults');
            if (wishlistNoResults) wishlistNoResults.classList.toggle('d-none', visible !== 0);
            // Accessibility: focus first matching product (optionally)
            if (visible > 0 && productsGrid) {
                const first = productsGrid.querySelector('.product-card-wrapper[style*="display:"]') || productsGrid.querySelector('.product-card-wrapper:not([style*="display: none"])');
                if (first) first.scrollIntoView({ behavior: 'smooth', block: 'center' });
            }
        }

        // SERVER-SIDE AJAX search (used when not on Shop page)
        function serverSideSearch(query) {
            if (!query || query.length < 2) {
                clearResults();
                hideLoading();
                hideNoResults();
                return;
            }

            showLoading();
            clearResults();
            hideNoResults();

            const url = `/Shop/SearchProducts?query=${encodeURIComponent(query)}`;
            fetch(url, {
                method: 'GET',
                headers: { 'X-Requested-With': 'XMLHttpRequest' },
                credentials: 'same-origin'
            })
                .then(response => {
                    const ct = response.headers.get('content-type') || '';
                    if (ct.includes('application/json')) {
                        return response.json().then(json => ({ ok: response.ok, status: response.status, json }));
                    }
                    return response.text().then(text => { throw new Error(`Server returned non-JSON response (status ${response.status}). Preview: ${text.substring(0, 800)}`); });
                })
                .then(({ ok, status, json }) => {
                    hideLoading();
                    if (!ok && status >= 400) {
                        const errMsg = (json && json.error) ? json.error : `Search request failed with status ${status}`;
                        clearResults();
                        if (searchResults) searchResults.innerHTML = `<div class="alert alert-danger m-3"><strong>Error:</strong> ${escapeHtml(errMsg)}</div>`;
                        return;
                    }
                    const products = json && (json.products || json.data || json.items) ? (json.products || json.data || json.items) : (Array.isArray(json) ? json : []);
                    renderResults(products);
                })
                .catch(err => {
                    hideLoading();
                    clearResults();
                    if (searchResults) {
                        const message = escapeHtml(err.message || 'Unknown error');
                        searchResults.innerHTML = `<div class="alert alert-danger m-3"><strong>Search Error:</strong> ${message}<br/><small>Open DevTools Network tab and inspect /Shop/SearchProducts response.</small></div>`;
                    }
                    console.error('custom-search fetch error:', err);
                });
        }

        // Unified performSearch: choose client or server path
        function performSearch(query) {
            const q = (query || '').trim();
            if (clientSideCards && clientSideCards.length > 0) {
                // Client-side: behave like wishlist
                clientSideFilter(q);
                // Also keep server results area clean
                clearResults();
                hideLoading();
            } else {
                // Fallback: server-side AJAX
                serverSideSearch(q);
            }
        }

        // Debounced input handler
        searchInput.addEventListener('input', function () {
            const q = this.value || '';
            clearTimeout(debounceTimer);
            if (q.trim().length < 2) {
                // quick reset for short queries
                performSearch(q);
                return;
            }
            debounceTimer = setTimeout(() => performSearch(q), DEBOUNCE_MS);
        });

        // Enter -> if server results exist navigate to first; if client-side navigate to first matching product
        searchInput.addEventListener('keydown', function (e) {
            if (e.key !== 'Enter') return;
            const q = this.value.trim();
            if (clientSideCards && clientSideCards.length > 0) {
                // find first visible matching card and navigate to its product page link
                const first = productsGrid.querySelector('.product-card-wrapper:not([style*="display: none"]) a') ||
                    productsGrid.querySelector('.product-card-wrapper a');
                if (first) {
                    e.preventDefault();
                    window.location.href = first.href;
                }
            } else {
                // server results: click first result link if present
                const firstLink = searchResults ? searchResults.querySelector('a') : null;
                if (firstLink) {
                    e.preventDefault();
                    window.location.href = firstLink.href;
                } else {
                    // if no results yet, trigger immediate server search
                    e.preventDefault();
                    performSearch(q);
                }
            }
        });

        // Prevent clicks inside popup content from closing popup
        const innerBoxes = searchPopup.querySelectorAll('.search-field, .search-result, .search-field__input, .search-field .btn');
        innerBoxes.forEach(el => el.addEventListener('click', e => e.stopPropagation()));

        // Initial debug log
        console.log('custom-search initialized', {
            clientSide: !!clientSideCards,
            productCount: clientSideCards ? clientSideCards.length : 0
        });
    });
})();