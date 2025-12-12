(function() {
    'use strict';
    
    document.addEventListener('DOMContentLoaded', function() {
        console.log('Custom search initialized');
        
        const searchPopup = document.querySelector('.search-popup');
        const searchInput = document.getElementById('headerSearchInput');
        const searchResults = document.getElementById('searchResults');
        const searchLoading = document.getElementById('searchLoading');
        const searchNoResults = document.getElementById('searchNoResults');
        const openSearchBtns = document.querySelectorAll('.js-search-popup');
        const closeSearchBtn = document.querySelector('.js-close-search');

        if (!searchPopup || !searchInput) {
            console.error('Search elements not found!');
            return;
        }

        let searchTimeout;

        // Open search popup
        openSearchBtns.forEach(btn => {
            btn.addEventListener('click', function(e) {
                e.preventDefault();
                e.stopPropagation();
                console.log('Opening search popup');
                searchPopup.style.display = 'block';
                document.body.style.overflow = 'hidden';
                setTimeout(() => searchInput.focus(), 100);
            });
        });

        // Close search popup
        function closeSearch() {
            console.log('Closing search popup');
            searchPopup.style.display = 'none';
            document.body.style.overflow = '';
            searchInput.value = '';
            searchResults.innerHTML = '';
            searchNoResults.classList.add('d-none');
            searchLoading.classList.add('d-none');
        }

        if (closeSearchBtn) {
            closeSearchBtn.addEventListener('click', closeSearch);
        }

        // Close on overlay click
        if (searchPopup) {
            searchPopup.addEventListener('click', function(e) {
                if (e.target === searchPopup) {
                    closeSearch();
                }
            });
        }

        // Close on ESC key
        document.addEventListener('keydown', function(e) {
            if (e.key === 'Escape' && searchPopup && searchPopup.style.display === 'block') {
                closeSearch();
            }
        });

        // Real-time search
        searchInput.addEventListener('input', function() {
            clearTimeout(searchTimeout);
            const query = this.value.trim();

            console.log('Search query:', query);

            if (query.length < 2) {
                searchResults.innerHTML = '';
                searchNoResults.classList.add('d-none');
                searchLoading.classList.add('d-none');
                return;
            }

            // Show loading
            searchLoading.classList.remove('d-none');
            searchResults.innerHTML = '';
            searchNoResults.classList.add('d-none');

            searchTimeout = setTimeout(function() {
                const url = `/Shop/SearchProducts?query=${encodeURIComponent(query)}`;
                console.log('Fetching:', url);

                fetch(url)
                    .then(response => {
                        console.log('Response status:', response.status);
                        if (!response.ok) {
                            throw new Error(`HTTP error! status: ${response.status}`);
                        }
                        return response.json();
                    })
                    .then(data => {
                        console.log('Search results:', data);
                        searchLoading.classList.add('d-none');

                        if (data.products && data.products.length > 0) {
                            let html = '<div class="list-group">';
                            data.products.forEach(product => {
                                const imageUrl = product.coverImageName || 'product-placeholder.jpg';
                                const price = product.basePrice ? product.basePrice.toFixed(2) : '0.00';
                                const category = product.categoryName || 'Uncategorized';
                                
                                html += `
                                    <a href="/Product/Details/${product.detailsUrl}" class="list-group-item list-group-item-action border-0 py-3">
                                        <div class="d-flex align-items-center gap-3">
                                            <img src="/assets/images/products/${imageUrl}" alt="${product.name}"
                                                 style="width: 70px; height: 70px; object-fit: cover; border-radius: 8px; flex-shrink: 0;"
                                                 onerror="this.src='/assets/images/no-image.png'">
                                            <div class="flex-grow-1 min-width-0">
                                                <h6 class="mb-1 text-truncate">${product.name}</h6>
                                                <p class="mb-0 text-muted small">${category}</p>
                                            </div>
                                            <div class="text-end flex-shrink-0">
                                                <strong class="text-primary fs-5">$${price}</strong>
                                            </div>
                                        </div>
                                    </a>
                                `;
                            });
                            html += '</div>';
                            searchResults.innerHTML = html;
                            searchNoResults.classList.add('d-none');
                        } else {
                            searchResults.innerHTML = '';
                            searchNoResults.classList.remove('d-none');
                        }
                    })
                    .catch(error => {
                        console.error('Search error:', error);
                        searchLoading.classList.add('d-none');
                        searchResults.innerHTML = `
                            <div class="alert alert-danger">
                                <strong>Error:</strong> ${error.message}
                            </div>
                        `;
                    });
            }, 300);
        });
    });
})();