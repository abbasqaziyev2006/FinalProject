(function () {
    'use strict';

    /**
     * Add to Basket Handler
     * Manages adding products to basket and updating cart count
     */
    const AddToBasketHandler = {
        basketCountSelector: '.js-cart-items-count',
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

            const productVariantId = this.getProductVariantId(productCard);

            if (!productVariantId) {
                console.warn('Product variant ID not found');
                return;
            }

            this.addToBasket(productVariantId, 1);
        },

        getProductVariantId(productCard) {
            const productWrapper = productCard.closest('.product-card-wrapper');
            const productId = productWrapper?.dataset?.productId;
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
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        // Update basket count immediately from the response
                        this.updateBasketCountFromData(data.totalCount);
                        this.showAddedNotification(data.message || 'Product added to basket!');
                    } else {
                        console.error('Failed to add item to basket');
                    }
                })
                .catch(error => {
                    console.error('Error adding to basket:', error);
                });
        },

        updateBasketCount() {
            fetch('/Basket/GetBasket')
                .then(response => response.json())
                .then(data => {
                    if (data.totalCount !== undefined) {
                        this.updateBasketCountFromData(data.totalCount);
                    }
                })
                .catch(error => console.error('Error updating basket count:', error));
        },

        updateBasketCountFromData(count) {
            const countElements = document.querySelectorAll(this.basketCountSelector);
            countElements.forEach(countElement => {
                if (countElement) {
                    countElement.textContent = count;
                    countElement.style.display = count > 0 ? 'inline-block' : 'none';
                }
            });
        },

        showAddedNotification(message) {
            // Create a simple toast notification
            const toast = document.createElement('div');
            toast.className = 'basket-toast';
            toast.textContent = message;
            toast.style.cssText = `
                position: fixed;
                top: 20px;
                right: 20px;
                background: #28a745;
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
    };

    // Add CSS animations
    const style = document.createElement('style');
    style.textContent = `
        @keyframes slideIn {
            from {
                transform: translateX(100%);
                opacity: 0;
            }
            to {
                transform: translateX(0);
                opacity: 1;
            }
        }
        @keyframes slideOut {
            from {
                transform: translateX(0);
                opacity: 1;
            }
            to {
                transform: translateX(100%);
                opacity: 0;
            }
        }
    `;
    document.head.appendChild(style);

    // Initialize when DOM is ready
    document.addEventListener('DOMContentLoaded', () => {
        AddToBasketHandler.init();
    });
})();