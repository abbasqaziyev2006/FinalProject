(function () {
    'use strict';

    const productGrid = document.getElementById('products-grid');

    const activeFilters = {
        categories: [],
        colors: [],
        sizes: [],
        brands: [],
        priceMin: 10,
        priceMax: 1000
    };

    // Initialize filters
    initializeFilters();

    function initializeFilters() {
        // Category filters
        document.querySelectorAll('[data-filter="category"]').forEach(link => {
            link.addEventListener('click', (e) => {
                e.preventDefault();
                const categoryId = link.dataset.categoryId;
                toggleFilter('categories', categoryId, link);
            });
        });

        // Color filters
        document.querySelectorAll('.swatch-color.js-filter').forEach(swatch => {
            swatch.addEventListener('click', (e) => {
                e.preventDefault();
                const color = window.getComputedStyle(swatch).backgroundColor;
                toggleFilter('colors', color, swatch);
            });
        });

        // Size filters
        document.querySelectorAll('.swatch-size.js-filter').forEach(button => {
            button.addEventListener('click', (e) => {
                e.preventDefault();
                const size = button.dataset.size;
                toggleFilter('sizes', size, button);
            });
        });

        // Brand filters (checkboxes)
        document.querySelectorAll('.brand-filter').forEach(checkbox => {
            checkbox.addEventListener('change', (e) => {
                const brand = checkbox.dataset.brand;
                const parentLabel = checkbox.closest('label');

                if (checkbox.checked) {
                    activeFilters.brands.push(brand);
                    parentLabel.classList.add('active');
                } else {
                    const index = activeFilters.brands.indexOf(brand);
                    if (index > -1) {
                        activeFilters.brands.splice(index, 1);
                    }
                    parentLabel.classList.remove('active');
                }

                applyFilters();
            });
        });

        // Price range filter
        const priceRangeInput = document.querySelector('.price-range-slider');
        if (priceRangeInput) {
            priceRangeInput.addEventListener('change', () => {
                const value = priceRangeInput.value;
                // Handle both comma and semicolon separated values
                const values = value.includes(',') ? value.split(',') : value.split(';');
                activeFilters.priceMin = parseInt(values[0]) || 10;
                activeFilters.priceMax = parseInt(values[1]) || 1000;
                updatePriceDisplay();
                applyFilters();
            });

            priceRangeInput.addEventListener('input', () => {
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

    function toggleFilter(filterType, filterValue, element) {
        const filterArray = activeFilters[filterType];
        const index = filterArray.indexOf(filterValue);

        if (index > -1) {
            // Remove filter
            filterArray.splice(index, 1);
            element.classList.remove('active', 'swatch_active');
        } else {
            // Add filter
            filterArray.push(filterValue);
            element.classList.add('active', 'swatch_active');
        }

        applyFilters();
    }

    function applyFilters() {
        const products = Array.from(productGrid.querySelectorAll('.product-card-wrapper'));
        let visibleCount = 0;

        products.forEach(productCard => {
            const product = parseProductData(productCard);
            const isVisible = checkFilters(product);

            if (isVisible) {
                productCard.style.display = '';
                productCard.classList.remove('d-none');
                visibleCount++;
            } else {
                productCard.style.display = 'none';
                productCard.classList.add('d-none');
            }
        });

        // Show/hide "no products" message
        updateNoProductsMessage(visibleCount);
    }

    function parseProductData(productCard) {
        // Get category from data attribute or from text
        const categoryText = productCard.querySelector('.pc__category')?.textContent.trim().toLowerCase() || '';
        const categoryAttr = productCard.dataset.productCategory?.toLowerCase() || '';
        const category = categoryAttr || categoryText;

        // Get price from data attribute or from text
        const priceText = productCard.querySelector('.product-card__price .price')?.textContent.replace('$', '') || '0';
        const priceAttr = productCard.dataset.productPrice || priceText;
        const price = parseFloat(priceAttr);

        // Get brand from data attribute or from hidden data
        const brandAttr = productCard.dataset.productBrand?.toLowerCase() || '';
        const brandData = productCard.querySelector('[data-brand]')?.dataset.brand?.toLowerCase() || '';
        const brand = brandAttr || brandData;

        return {
            category: category,
            title: productCard.querySelector('.pc__title')?.textContent.trim().toLowerCase() || '',
            price: price,
            sizes: Array.from(productCard.querySelectorAll('[data-size]')).map(el => el.dataset.size),
            colors: Array.from(productCard.querySelectorAll('[data-color]')).map(el => el.dataset.color),
            brand: brand,
            element: productCard
        };
    }

    function checkFilters(product) {
        // Check category filter
        if (activeFilters.categories.length > 0) {
            const categoryMatch = activeFilters.categories.some(cat =>
                product.category.includes(cat.toLowerCase())
            );
            if (!categoryMatch) return false;
        }

        // Check color filter
        if (activeFilters.colors.length > 0) {
            const colorMatch = activeFilters.colors.some(color => {
                // Convert both to RGB for comparison
                return rgbMatch(color, product.colors);
            });
            if (!colorMatch) return false;
        }

        // Check size filter
        if (activeFilters.sizes.length > 0) {
            const sizeMatch = activeFilters.sizes.some(size =>
                product.sizes.includes(size.toLowerCase())
            );
            if (!sizeMatch) return false;
        }

        // Check brand filter
        if (activeFilters.brands.length > 0) {
            const brandMatch = activeFilters.brands.some(brand =>
                product.brand.includes(brand.toLowerCase())
            );
            if (!brandMatch) return false;
        }

        // Check price filter
        if (product.price < activeFilters.priceMin || product.price > activeFilters.priceMax) {
            return false;
        }

        return true;
    }

    function rgbMatch(selectedColor, productColors) {
        // If no product colors, skip color filter
        if (productColors.length === 0) return true;

        // Compare hex colors directly first
        return productColors.some(color => {
            return color.toLowerCase() === selectedColor.toLowerCase();
        });
    }

    function updatePriceDisplay() {
        document.querySelector('.price-range__min').textContent = `$${activeFilters.priceMin}`;
        document.querySelector('.price-range__max').textContent = `$${activeFilters.priceMax}`;
    }

    function updateNoProductsMessage(visibleCount) {
        let noProductsMsg = productGrid.parentNode.querySelector('.no-products-message');

        if (!noProductsMsg) {
            noProductsMsg = document.createElement('div');
            noProductsMsg.className = 'no-products-message text-center py-5';
            noProductsMsg.innerHTML = `
                <p class="text-secondary fw-medium">No products found matching your filters</p>
                <button class="btn btn-outline-dark btn-sm mt-3 js-reset-filters">Reset Filters</button>
            `;
            productGrid.parentNode.appendChild(noProductsMsg);

            noProductsMsg.querySelector('.js-reset-filters').addEventListener('click', (e) => {
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
        activeFilters.priceMin = 10;
        activeFilters.priceMax = 1000;

        // Remove active classes from all filter elements
        document.querySelectorAll('[data-filter="category"]').forEach(el => {
            el.classList.remove('active', 'swatch_active');
        });

        document.querySelectorAll('.swatch-color.js-filter').forEach(el => {
            el.classList.remove('active', 'swatch_active');
        });

        document.querySelectorAll('.swatch-size.js-filter').forEach(el => {
            el.classList.remove('active', 'swatch_active');
        });

        document.querySelectorAll('.brand-filter').forEach(checkbox => {
            checkbox.checked = false;
            checkbox.closest('label').classList.remove('active');
        });

        // Reset price display
        updatePriceDisplay();

        // Reset price slider if it exists
        const priceRangeInput = document.querySelector('.price-range-slider');
        if (priceRangeInput) {
            priceRangeInput.value = '[10,1000]';
        }

        // Apply filters to show all products
        applyFilters();
    }

    // Expose reset function globally
    window.resetFilters = resetFilters;
})();