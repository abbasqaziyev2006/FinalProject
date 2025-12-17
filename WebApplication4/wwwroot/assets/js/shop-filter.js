(function () {
    'use strict';

    const productGrid = document.getElementById('products-grid');
    if (!productGrid) return;

    const activeFilters = {
        categories: [],
        colors: [],
        sizes: [],
        brands: [],
        priceMin: 10,
        priceMax: 1000
    };

    let allLoadedProducts = [];
    let isLoading = false;
    let hasMoreProducts = true;
    let currentSkip = 0;
    const pageSize = 12;

    // Initialize everything
    initializeProducts();
    initializeFilters();
    initializeInfiniteScroll();
    initializeSorting();
    initializeAddToCart();

    function initializeProducts() {
        // Store initially loaded products with ALL their data
        document.querySelectorAll('.product-card-wrapper').forEach(function (wrapper) {
            const colorIds = wrapper.dataset.productColors;
            const sizes = wrapper.dataset.productSizes;

            const productData = {
                element: wrapper,
                id: wrapper.dataset.productId || '',
                category: wrapper.dataset.productCategory || '',
                brand: parseInt(wrapper.dataset.productBrand) || 0,
                colors: colorIds ? colorIds.toString().split(',').map(id => parseInt(id)).filter(id => !isNaN(id)) : [],
                sizes: sizes ? sizes.toString().split(',').map(s => s.trim().toUpperCase()).filter(s => s) : [],
                price: parseFloat(wrapper.dataset.productPrice) || 0,
                name: wrapper.dataset.productName || '',
                date: wrapper.dataset.productDate || wrapper.dataset.productId || ''
            };

            allLoadedProducts.push(productData);
        });

        currentSkip = allLoadedProducts.length;
        console.log('Initialized products:', allLoadedProducts.length);
    }

    function initializeFilters() {
        // Category filters
        document.querySelectorAll('.category-filter').forEach(link => {
            link.addEventListener('click', (e) => {
                e.preventDefault();

                // Remove active from all
                document.querySelectorAll('.category-filter').forEach(el => {
                    el.classList.remove('text-primary', 'fw-bold', 'active');
                });

                // Add active to clicked
                link.classList.add('text-primary', 'fw-bold', 'active');

                const categoryId = link.dataset.categoryId;

                if (categoryId === 'all') {
                    activeFilters.categories = [];
                } else {
                    activeFilters.categories = [categoryId];
                }

                console.log('Category filter:', categoryId);
                applyFilters();
            });
        });

        // Color filters
        document.querySelectorAll('.color-filter').forEach(checkbox => {
            checkbox.addEventListener('change', (e) => {
                const label = checkbox.closest('label');
                const swatch = label.querySelector('.swatch-color');
                const check = label.querySelector('.color-check');
                const colorId = parseInt(checkbox.dataset.colorId);

                if (checkbox.checked) {
                    if (swatch) {
                        swatch.style.borderColor = '#000';
                        swatch.style.boxShadow = '0 0 0 2px #000';
                    }
                    if (check) check.classList.remove('d-none');

                    if (!activeFilters.colors.includes(colorId)) {
                        activeFilters.colors.push(colorId);
                    }
                } else {
                    if (swatch) {
                        swatch.style.borderColor = '#e0e0e0';
                        swatch.style.boxShadow = 'none';
                    }
                    if (check) check.classList.add('d-none');

                    const index = activeFilters.colors.indexOf(colorId);
                    if (index > -1) {
                        activeFilters.colors.splice(index, 1);
                    }
                }

                console.log('Color filter:', activeFilters.colors);
                applyFilters();
            });
        });

        // Size filters
        document.querySelectorAll('.size-filter').forEach(checkbox => {
            checkbox.addEventListener('change', (e) => {
                const label = checkbox.closest('label');
                const badge = label.querySelector('.size-badge');
                const size = checkbox.dataset.size.toString().toUpperCase();

                if (checkbox.checked) {
                    if (badge) {
                        badge.style.backgroundColor = '#000';
                        badge.style.color = '#fff';
                        badge.style.borderColor = '#000';
                    }

                    if (!activeFilters.sizes.includes(size)) {
                        activeFilters.sizes.push(size);
                    }
                } else {
                    if (badge) {
                        badge.style.backgroundColor = 'transparent';
                        badge.style.color = '#000';
                        badge.style.borderColor = '#ddd';
                    }

                    const index = activeFilters.sizes.indexOf(size);
                    if (index > -1) {
                        activeFilters.sizes.splice(index, 1);
                    }
                }

                console.log('Size filter:', activeFilters.sizes);
                applyFilters();
            });
        });

        // Brand filters
        document.querySelectorAll('.brand-filter').forEach(checkbox => {
            checkbox.addEventListener('change', (e) => {
                const brandId = parseInt(checkbox.dataset.brandId);

                if (checkbox.checked) {
                    if (!activeFilters.brands.includes(brandId)) {
                        activeFilters.brands.push(brandId);
                    }
                } else {
                    const index = activeFilters.brands.indexOf(brandId);
                    if (index > -1) {
                        activeFilters.brands.splice(index, 1);
                    }
                }

                console.log('Brand filter:', activeFilters.brands);
                applyFilters();
            });
        });

        // Price range filter
        const priceRangeInput = document.querySelector('.price-range-slider');
        if (priceRangeInput) {
            priceRangeInput.addEventListener('change', () => {
                const value = priceRangeInput.value;
                const values = value.includes(',') ? value.split(',') : value.split(';');
                activeFilters.priceMin = parseInt(values[0]) || 10;
                activeFilters.priceMax = parseInt(values[1]) || 1000;
                updatePriceDisplay();
                applyFilters();
            });
        }

        // Reset filters button
        document.querySelectorAll('.js-reset-filters').forEach(button => {
            button.addEventListener('click', (e) => {
                e.preventDefault();
                resetFilters();
            });
        });
    }

    function applyFilters() {
        let visibleCount = 0;

        allLoadedProducts.forEach(product => {
            const isVisible = checkProductAgainstFilters(product);

            if (isVisible) {
                product.element.style.display = '';
                product.element.classList.remove('d-none');
                visibleCount++;
            } else {
                product.element.style.display = 'none';
                product.element.classList.add('d-none');
            }
        });

        // Update products count
        const productsCountEl = document.getElementById('productsCount');
        if (productsCountEl) {
            productsCountEl.textContent = visibleCount;
        }

        // Show/hide "no products" message
        updateNoProductsMessage(visibleCount);

        console.log('Filtered:', visibleCount, '/', allLoadedProducts.length);
    }

    function checkProductAgainstFilters(product) {
        // Category filter - MOST IMPORTANT FIX
        if (activeFilters.categories.length > 0) {
            const categoryMatch = activeFilters.categories.some(cat => {
                // Try exact match first
                if (product.category === cat) return true;
                // Try string comparison
                if (product.category.toString() === cat.toString()) return true;
                // Try loose equals
                if (product.category == cat) return true;
                return false;
            });

            if (!categoryMatch) {
                console.log('❌ Category mismatch:', product.name, 'has:', product.category, 'need:', activeFilters.categories);
                return false;
            }
        }

        // Brand filter
        if (activeFilters.brands.length > 0) {
            const brandMatch = activeFilters.brands.includes(product.brand);
            if (!brandMatch) {
                console.log('❌ Brand mismatch:', product.name);
                return false;
            }
        }

        // Color filter
        if (activeFilters.colors.length > 0) {
            // Check if product has ANY of the selected colors
            const colorMatch = activeFilters.colors.some(colorId =>
                product.colors.includes(colorId)
            );
            if (!colorMatch) {
                console.log('❌ Color mismatch:', product.name, 'has:', product.colors, 'need any of:', activeFilters.colors);
                return false;
            }
        }

        // Size filter
        if (activeFilters.sizes.length > 0) {
            const sizeMatch = activeFilters.sizes.some(size =>
                product.sizes.includes(size)
            );
            if (!sizeMatch) {
                console.log('❌ Size mismatch:', product.name);
                return false;
            }
        }

        // Price filter
        if (product.price < activeFilters.priceMin || product.price > activeFilters.priceMax) {
            console.log('❌ Price mismatch:', product.name);
            return false;
        }

        console.log('✅ Match:', product.name);
        return true;
    }

    function updatePriceDisplay() {
        const minEl = document.querySelector('.price-range__min');
        const maxEl = document.querySelector('.price-range__max');

        if (minEl) minEl.textContent = `$${activeFilters.priceMin}`;
        if (maxEl) maxEl.textContent = `$${activeFilters.priceMax}`;
    }

    function updateNoProductsMessage(visibleCount) {
        let noProductsMsg = productGrid.parentNode.querySelector('.no-products-message');

        if (!noProductsMsg) {
            noProductsMsg = document.createElement('div');
            noProductsMsg.className = 'no-products-message text-center py-5';
            noProductsMsg.innerHTML = `
                <p class="text-secondary fw-medium">No products found matching your filters</p>
                <button class="btn btn-outline-dark btn-sm mt-3 js-reset-filters-msg">Reset Filters</button>
            `;
            productGrid.parentNode.appendChild(noProductsMsg);

            noProductsMsg.querySelector('.js-reset-filters-msg').addEventListener('click', (e) => {
                e.preventDefault();
                resetFilters();
            });
        }

        noProductsMsg.style.display = visibleCount === 0 ? 'block' : 'none';
    }

    function resetFilters() {
        // Clear all filters
        activeFilters.categories = [];
        activeFilters.colors = [];
        activeFilters.sizes = [];
        activeFilters.brands = [];

        // Get price range from products
        if (allLoadedProducts.length > 0) {
            const prices = allLoadedProducts.map(p => p.price).filter(p => p > 0);
            activeFilters.priceMin = prices.length > 0 ? Math.min(...prices) : 10;
            activeFilters.priceMax = prices.length > 0 ? Math.max(...prices) : 1000;
        } else {
            activeFilters.priceMin = 10;
            activeFilters.priceMax = 1000;
        }

        // Reset UI - Categories
        document.querySelectorAll('.category-filter').forEach(el => {
            el.classList.remove('text-primary', 'fw-bold', 'active');
        });
        const allCategoriesLink = document.querySelector('.category-filter[data-category-id="all"]');
        if (allCategoriesLink) {
            allCategoriesLink.classList.add('text-primary', 'fw-bold', 'active');
        }

        // Reset UI - Colors
        document.querySelectorAll('.color-filter').forEach(checkbox => {
            checkbox.checked = false;
            const label = checkbox.closest('label');
            const swatch = label.querySelector('.swatch-color');
            const check = label.querySelector('.color-check');

            if (swatch) {
                swatch.style.borderColor = '#e0e0e0';
                swatch.style.boxShadow = 'none';
            }
            if (check) check.classList.add('d-none');
        });

        // Reset UI - Sizes
        document.querySelectorAll('.size-filter').forEach(checkbox => {
            checkbox.checked = false;
            const label = checkbox.closest('label');
            const badge = label.querySelector('.size-badge');

            if (badge) {
                badge.style.backgroundColor = 'transparent';
                badge.style.color = '#000';
                badge.style.borderColor = '#ddd';
            }
        });

        // Reset UI - Brands
        document.querySelectorAll('.brand-filter').forEach(checkbox => {
            checkbox.checked = false;
        });

        // Reset price display
        updatePriceDisplay();

        // Reset sorting
        const sortSelect = document.getElementById('sortProducts');
        if (sortSelect) sortSelect.value = 'default';

        // Apply filters
        applyFilters();
    }

    function initializeInfiniteScroll() {
        window.addEventListener('scroll', function () {
            if (isLoading || !hasMoreProducts) return;

            const scrollTop = window.pageYOffset || document.documentElement.scrollTop;
            const windowHeight = window.innerHeight;
            const documentHeight = document.documentElement.scrollHeight;
            const scrollPercentage = (scrollTop + windowHeight) / documentHeight;

            if (scrollPercentage > 0.8) {
                loadMoreProducts();
            }
        });
    }

    function loadMoreProducts() {
        if (isLoading || !hasMoreProducts) return;
        isLoading = true;

        const loadingIndicator = document.getElementById('loading-indicator');
        if (loadingIndicator) loadingIndicator.style.display = 'block';

        fetch(`/Shop/LoadMoreProducts?skip=${currentSkip}&take=${pageSize}`)
            .then(response => response.json())
            .then(data => {
                if (!data.hasMore || data.products.length === 0) {
                    hasMoreProducts = false;
                    const noMoreMsg = document.getElementById('no-more-products');
                    if (noMoreMsg) noMoreMsg.style.display = 'block';
                    return;
                }

                // Add products (you'll need to implement createProductCard)
                data.products.forEach(product => {
                    // Add to allLoadedProducts array
                    // Then apply current filters
                });

                currentSkip += pageSize;
                applyFilters();
            })
            .catch(error => console.error('Error loading products:', error))
            .finally(() => {
                isLoading = false;
                if (loadingIndicator) loadingIndicator.style.display = 'none';
            });
    }

    function initializeSorting() {
        const sortSelect = document.getElementById('sortProducts');
        if (!sortSelect) return;

        sortSelect.addEventListener('change', function () {
            const sortBy = this.value;
            sortProducts(sortBy);
        });
    }

    function sortProducts(sortBy) {
        // Get visible products only
        const visibleProducts = allLoadedProducts.filter(p =>
            p.element.style.display !== 'none' && !p.element.classList.contains('d-none')
        );

        let sorted = [...visibleProducts];

        switch (sortBy) {
            case 'name-asc':
                sorted.sort((a, b) => a.name.localeCompare(b.name));
                break;
            case 'name-desc':
                sorted.sort((a, b) => b.name.localeCompare(a.name));
                break;
            case 'price-asc':
                sorted.sort((a, b) => a.price - b.price);
                break;
            case 'price-desc':
                sorted.sort((a, b) => b.price - a.price);
                break;
            case 'date-asc':
                sorted.sort((a, b) => (a.date || 0) - (b.date || 0));
                break;
            case 'date-desc':
                sorted.sort((a, b) => (b.date || 0) - (a.date || 0));
                break;
        }

        // Reorder in DOM
        sorted.forEach(product => {
            productGrid.appendChild(product.element);
        });
    }

    function initializeAddToCart() {
        document.addEventListener('click', function (e) {
            if (!e.target.classList.contains('js-add-cart')) return;
            e.preventDefault();

            const button = e.target;
            const variantId = button.dataset.variantId;
            if (!variantId) return;

            button.disabled = true;
            const originalText = button.textContent;

            const formData = new FormData();
            formData.append('productVariantId', variantId);
            formData.append('quantity', 1);

            fetch('/Basket/Add', {
                method: 'POST',
                body: formData
            })
                .then(response => response.json())
                .then(data => {
                    // Update cart count
                    if (data && data.totalCount !== undefined) {
                        document.querySelectorAll('.js-cart-items-count, .header__cart-count').forEach(el => {
                            el.textContent = data.totalCount;
                            el.style.display = '';
                        });
                    }
                    showNotification('Product added to cart!', 'success');
                })
                .catch(error => {
                    console.error('Error:', error);
                    showNotification('Error adding to cart', 'error');
                })
                .finally(() => {
                    button.disabled = false;
                    button.textContent = originalText;
                });
        });
    }

    function showNotification(message, type) {
        const toast = document.createElement('div');
        toast.className = 'basket-toast';
        toast.textContent = message;
        toast.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            background: ${type === 'success' ? '#28a745' : '#dc3545'};
            color: white;
            padding: 15px 25px;
            border-radius: 5px;
            z-index: 10000;
            box-shadow: 0 4px 6px rgba(0,0,0,0.1);
            animation: slideIn 0.3s ease-out;
        `;

        document.body.appendChild(toast);

        setTimeout(() => {
            toast.style.animation = 'slideOut 0.3s ease-out';
            setTimeout(() => toast.remove(), 300);
        }, 2000);
    }

    // Debug function
    window.debugFilters = function () {
        console.log('=== DEBUG INFO ===');
        console.log('Active Filters:', activeFilters);
        console.log('Total Products:', allLoadedProducts.length);
        console.log('Products:', allLoadedProducts.map(p => ({
            name: p.name,
            category: p.category,
            brand: p.brand,
            colors: p.colors,
            sizes: p.sizes,
            price: p.price
        })));

        const missing = allLoadedProducts.filter(p => !p.category || !p.name);
        if (missing.length > 0) {
            console.warn('Products with missing data:', missing);
        }
    };

    window.resetFilters = resetFilters;
})();