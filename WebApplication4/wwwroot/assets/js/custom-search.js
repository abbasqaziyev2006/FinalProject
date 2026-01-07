(function () {
    'use strict';

    /**
     * custom-search.js - header search now behaves like wishlist search (renders product grid in popup).
     */

    document.addEventListener('DOMContentLoaded', function () {
        // Elements
        const searchPopup = document.querySelector('.search-popup');
        const searchInput = document.getElementById('headerSearchInput');
        const searchResults = document.getElementById('searchResults');
        const searchLoading = document.getElementById('searchLoading');
        const searchNoResults = document.getElementById('searchNoResults');
        const openSearchBtns = Array.from(document.querySelectorAll('.js-search-popup'));
        const closeSearchBtn = document.querySelector('.js-close-search');

        // Selection / actions UI (may be present in header partial)
        const selectionBar = document.getElementById('searchSelectionBar');
        const selectAllCheckbox = document.getElementById('searchSelectAll');
        const selectedCountEl = document.getElementById('searchSelectedCount');
        const btnAddSelectedToWishlist = document.getElementById('btnAddSelectedToWishlist');
        const btnAddSelectedToCart = document.getElementById('btnAddSelectedToCart');
        const btnViewAllOnShop = document.getElementById('btnViewAllOnShop');

        if (!searchPopup || !searchInput || !searchResults) return;

        // Detect local products grid (Shop page)
        const productsGrid = document.getElementById('products-grid');
        let clientSideCards = productsGrid ? Array.from(productsGrid.querySelectorAll('.product-card-wrapper')) : null;

        const DEBOUNCE_MS = 300;
        let debounceTimer = null;

        function showLoading() { if (searchLoading) searchLoading.classList.remove('d-none'); }
        function hideLoading() { if (searchLoading) searchLoading.classList.add('d-none'); }
        function showNoResults() { if (searchNoResults) searchNoResults.classList.remove('d-none'); }
        function hideNoResults() { if (searchNoResults) searchNoResults.classList.add('d-none'); }
        function clearResults() { if (searchResults) searchResults.innerHTML = ''; hideSelectionBar(); }

        // Selection storage per query (kept for compatibility but header grid will not show checkboxes)
        const STORAGE_PREFIX = 'searchSelection_';
        let selectedProducts = new Set();
        let currentQueryForSelection = '';
        function saveSelectionForQuery(query) {
            if (!query) return;
            try { sessionStorage.setItem(STORAGE_PREFIX + query, JSON.stringify(Array.from(selectedProducts))); } catch (e) { }
        }
        function loadSelectionForQuery(query) {
            selectedProducts.clear();
            if (!query) return;
            try {
                const raw = sessionStorage.getItem(STORAGE_PREFIX + query);
                if (raw) JSON.parse(raw).forEach(id => selectedProducts.add(String(id)));
            } catch (e) { }
        }

        function showSelectionBar() { if (!selectionBar) return; selectionBar.classList.remove('d-none'); selectionBar.style.display = 'flex'; }
        function hideSelectionBar() { if (!selectionBar) return; selectionBar.classList.add('d-none'); selectionBar.style.display = 'none'; if (selectAllCheckbox) selectAllCheckbox.checked = false; updateSelectedCount(); }
        function updateSelectedCount() { if (!selectedCountEl) return; selectedCountEl.textContent = `${selectedProducts.size} selected`; const disabled = selectedProducts.size === 0; if (btnAddSelectedToWishlist) btnAddSelectedToWishlist.disabled = disabled; if (btnAddSelectedToCart) btnAddSelectedToCart.disabled = disabled; }

        // Helpers
        function escapeHtml(str) { if (!str) return ''; return String(str).replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;').replace(/'/g, '&#039;'); }

        // Render results as a product grid (wishlist-like)
        function renderResultsGrid(products, query) {
            clearResults();
            hideNoResults();

            if (!Array.isArray(products) || products.length === 0) {
                showNoResults();
                return;
            }

            // hide selection UI for grid mode (optional)
            if (selectionBar) hideSelectionBar();

            // container similar to wishlist grid
            const wrapper = document.createElement('div');
            wrapper.className = 'products-grid row row-cols-1 row-cols-md-2 row-cols-lg-3 g-3';

            products.forEach(p => {
                try {
                    const id = p.id || '';
                    const name = p.name || 'Unknown';
                    const image = p.coverImageName || p.image || 'product-placeholder.jpg';
                    const category = p.categoryName || p.category || '';
                    const priceVal = (p.basePrice !== undefined) ? p.basePrice : (p.price || p.salePrice || 0);
                    const price = (typeof priceVal === 'number') ? priceVal.toFixed(2) : parseFloat(priceVal || 0).toFixed(2);
                    const detailsUrl = p.detailsUrl || p.url || p.slug || id;

                    const col = document.createElement('div');
                    col.className = 'product-card-wrapper col';

                    // minimal product card markup matching wishlist style
                    col.innerHTML = `
                        <div class="product-card mb-3 mb-md-4 mb-xxl-5">
                            <div class="pc__img-wrapper">
                                <a href="/Product/Details/${encodeURIComponent(detailsUrl)}" class="d-block">
                                    <img loading="lazy" src="/images/products/${encodeURIComponent(image)}" alt="${escapeHtml(name)}" class="pc__img" style="width:100%;height:auto;object-fit:cover;"/>
                                </a>
                            </div>
                            <div class="pc__info position-relative">
                                <p class="pc__category">${escapeHtml(category)}</p>
                                <h6 class="pc__title"><a href="/Product/Details/${encodeURIComponent(detailsUrl)}">${escapeHtml(name)}</a></h6>
                                <div class="product-card__price d-flex">
                                    <span class="money price">${escapeHtml(price)}</span>
                                </div>
                            </div>
                        </div>
                    `;
                    wrapper.appendChild(col);
                } catch (err) {
                    console.warn('renderResultsGrid error', err, p);
                }
            });

            if (searchResults) searchResults.appendChild(wrapper);

            // Update "View all" link
            if (btnViewAllOnShop) btnViewAllOnShop.href = `/Shop?search=${encodeURIComponent(query || '')}`;

            // focus first product link for keyboard users
            setTimeout(() => {
                const firstLink = searchResults.querySelector('a');
                if (firstLink) firstLink.focus();
            }, 120);
        }

        // Original list-style results (kept as fallback)
        function renderResultsList(products, query) {
            clearResults();
            hideNoResults();

            if (!Array.isArray(products) || products.length === 0) {
                showNoResults();
                return;
            }

            // show selection bar (if present) and load selection state
            currentQueryForSelection = String(query || '').trim();
            loadSelectionForQuery(currentQueryForSelection);
            if (selectionBar) showSelectionBar();

            const wrapper = document.createElement('div');
            wrapper.className = 'list-group list-group-flush';

            products.forEach(p => {
                const item = document.createElement('div');
                item.className = 'search-result-row list-group-item border-0 py-2 d-flex align-items-center gap-3';
                const id = p.id || '';
                const name = p.name || 'Unknown';
                const image = p.coverImageName || p.image || 'product-placeholder.jpg';
                const category = p.categoryName || p.category || '';
                const priceVal = (p.basePrice !== undefined) ? p.basePrice : (p.price || p.salePrice || 0);
                const price = (typeof priceVal === 'number') ? priceVal.toFixed(2) : parseFloat(priceVal || 0).toFixed(2);
                const detailsUrl = p.detailsUrl || p.url || p.slug || id;

                item.innerHTML = `
                    <div style="flex:0 0 28px;">
                        <input type="checkbox" class="search-select-checkbox form-check-input" data-product-id="${escapeHtml(id)}" />
                    </div>
                    <a href="/Product/Details/${encodeURIComponent(detailsUrl)}" class="flex-grow-1 text-decoration-none" style="min-width:0;">
                        <div class="d-flex align-items-center gap-3">
                            <img src="/images/products/${encodeURIComponent(image)}" alt="${escapeHtml(name)}" style="width:70px;height:70px;object-fit:cover;border-radius:8px;flex-shrink:0;" />
                            <div class="flex-grow-1" style="min-width:0;">
                                <h6 class="mb-1 text-truncate" title="${escapeHtml(name)}" style="margin:0;">${escapeHtml(name)}</h6>
                                <p class="mb-0 text-muted small text-truncate" title="${escapeHtml(category)}" style="margin:0;">${escapeHtml(category)}</p>
                            </div>
                            <div class="text-end" style="flex-shrink:0;">
                                <strong class="text-primary fs-6">$${price}</strong>
                            </div>
                        </div>
                    </a>
                `;
                wrapper.appendChild(item);
            });

            searchResults.appendChild(wrapper);
        }

        // Decide which renderer to use; header popup uses grid to match wishlist
        function renderResults(products, query) {
            renderResultsGrid(products, query);
        }

        // Debounced input handler
        searchInput.addEventListener('input', function () {
            const q = this.value || '';
            clearTimeout(debounceTimer);
            if (q.trim().length < 2) {
                clearResults();
                hideLoading();
                hideNoResults();
                return;
            }
            debounceTimer = setTimeout(() => performSearch(q), DEBOUNCE_MS);
        });

        // Unified performSearch: client-side on Shop page, otherwise server-side AJAX
        function performSearch(query) {
            const q = (query || '').trim();
            if (!q) { clearResults(); hideLoading(); hideNoResults(); return; }

            if (clientSideCards && clientSideCards.length > 0) {
                // If user is on Shop page, mimic wishlist filter: show matching product cards in popup
                const matched = [];
                clientSideCards.forEach(card => {
                    const title = (card.querySelector('.pc__title')?.textContent || '').trim();
                    const category = (card.querySelector('.pc__category')?.textContent || '').trim();
                    const match = title.toLowerCase().includes(q.toLowerCase()) || category.toLowerCase().includes(q.toLowerCase());
                    if (match) {
                        const product = {
                            id: card.dataset.productId,
                            name: title,
                            coverImageName: card.querySelector('.main-product-image')?.getAttribute('src')?.split('/').pop() || '',
                            categoryName: category,
                            basePrice: parseFloat(card.dataset.productPrice) || 0,
                            detailsUrl: card.querySelector('a')?.getAttribute('href')?.split('/Product/Details/')[1] || card.dataset.productId
                        };
                        matched.push(product);
                    }
                });
                renderResults(matched, q);
            } else {
                serverSideSearch(q);
            }
        }

        // Server-side AJAX search
        function serverSideSearch(query) {
            if (!query || query.length < 2) { clearResults(); hideLoading(); hideNoResults(); return; }

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
                    return response.text().then(text => { throw new Error(`Server returned non-JSON response (status ${response.status}).`); });
                })
                .then(({ ok, status, json }) => {
                    hideLoading();
                    const products = json && (json.products 