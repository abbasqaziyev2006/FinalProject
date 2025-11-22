(function () {
    'use strict';

    const sortSelect = document.querySelector('.shop-acs__select');
    const productsGrid = document.getElementById('products-grid');
    const loadMoreBtn = document.getElementById('loadMoreBtn');
    
    if (!sortSelect) return;

    // Initialize with default sorting
    sortSelect.value = 'default';
    
    // Listen for sort changes
    sortSelect.addEventListener('change', function () {
        const sortValue = this.value;
        sortProducts(sortValue);
    });

    function sortProducts(sortType) {
        const products = Array.from(productsGrid.querySelectorAll('.product-card-wrapper'));
        
        products.sort((a, b) => {
            const getPrice = (el) => {
                const priceText = el.querySelector('.product-card__price .price');
                return parseFloat(priceText ? priceText.textContent.replace('$', '') : 0);
            };

            const getTitle = (el) => {
                const titleEl = el.querySelector('.pc__title a');
                return titleEl ? titleEl.textContent.trim().toLowerCase() : '';
            };

            const getDate = (el) => {
                // Assuming newer products are added later (DOM order)
                return Array.from(products).indexOf(el);
            };

            switch (sortType) {
                case 'featured':
                    // Reset to original order
                    return 0;
                
                case 'best-selling':
                    // This would typically come from the server
                    // For now, we'll sort by review count if available
                    const reviewsA = el.querySelector('.reviews-note');
                    const reviewsB = el.querySelector('.reviews-note');
                    return 0;
                
                case 'alphabetic-az':
                    return getTitle(a).localeCompare(getTitle(b));
                
                case 'alphabetic-za':
                    return getTitle(b).localeCompare(getTitle(a));
                
                case 'price-low-high':
                    return getPrice(a) - getPrice(b);
                
                case 'price-high-low':
                    return getPrice(b) - getPrice(a);
                
                case 'date-old-new':
                    return getDate(a) - getDate(b);
                
                case 'date-new-old':
                    return getDate(b) - getDate(a);
                
                default:
                    return 0;
            }
        });

        // Clear the grid and re-append sorted products
        productsGrid.innerHTML = '';
        products.forEach(product => {
            productsGrid.appendChild(product);
        });

        // Reinitialize sliders for re-ordered products
        if (typeof UomoSections !== 'undefined' && UomoSections.SwiperSlideshow) {
            setTimeout(() => {
                new UomoSections.SwiperSlideshow();
            }, 100);
        }
    }
})();