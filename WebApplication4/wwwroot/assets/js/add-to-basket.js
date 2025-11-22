(function () {
    'use strict';

    /**
     * Add to Basket Handler
     * Manages adding products to basket and updating cart count
     */
    const AddToBasketHandler = {
        basketCountSelector: '.header__cart-count', // Adjust based on your header structure
        addCartButtonClass: '.js-add-cart',
        
        init() {
            this.attachEventListeners();
            this.updateBasketCount();
        },

        attachEventListeners() {
            document.addEventListener('click', (e) => {
                if (e.target.closest(this.addCartButtonClass)) {
                    e.preventDefault();
                    this.handleAddToBasket(e.target.closest(this.addCartButtonClass));
                }
            });
        },

        handleAddToBasket(button) {
            const productCard = button.closest('.product-card');
            if (!productCard) return;

            // Get product variant ID from product card (you may need to adjust this based on your HTML structure)
            const productVariantId = this.getProductVariantId(productCard);
            
            if (!productVariantId) {
                console.warn('Product variant ID not found');
                return;
            }

            // Add to basket with quantity 1
            this.addToBasket(productVariantId, 1);
        },

        getProductVariantId(productCard) {
            // Try to get product variant ID from data attribute, or from product ID
            // Adjust this based on your actual HTML structure
            const productId = productCard.dataset.productId || 
                            productCard.querySelector('a[href*="/Product/Details/"]')?.href?.split('/').pop();
            
            // For now, return a default value - you may need to fetch the variant ID from API
            return productId ? parseInt(productId) : null;
        },

        addToBasket(productVariantId, quantity) {
            const formData = new FormData();
            formData.append('productVariantId', productVariantId);
            formData.append('quantity', quantity);

            fetch('/Basket/Add', {
                method: 'POST',
                body: formData
            })
            .then(response => {
                if (response.ok) {
                    this.updateBasketCount();
                    this.showAddedNotification();
                } else {
                    console.error('Failed to add item to basket');
                }
            })
            .catch(error => console.error('Error adding to basket:', error));
        },

        updateBasketCount() {
            fetch('/Basket/GetBasket')
                .then(response => response.json())
                .then(data => {
                    const countElement = document.querySelector(this.basketCountSelector);
                    if (countElement && data.totalCount !== undefined) {
                        countElement.textContent = data.totalCount;
                        countElement.style.display = data.totalCount > 0 ? 'inline-block' : 'none';
                    }
                })
                .catch(error => console.error('Error updating basket count:', error));
        },

        showAddedNotification() {
            // Optional: Show a toast notification
            const message = 'Product added to basket!';
            if (typeof showNotification === 'function') {
                showNotification(message, 'success');
            } else {
                console.log(message);
            }
        }
    };

    // Initialize when DOM is ready
    document.addEventListener('DOMContentLoaded', () => {
        AddToBasketHandler.init();
    });
})();