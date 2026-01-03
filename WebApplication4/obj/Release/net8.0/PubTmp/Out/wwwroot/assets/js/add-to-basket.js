(function () {
    'use strict';

    console.log('🔧 Add to Basket script loaded');

    /**
     * Add to Basket Handler
     * Manages adding products to basket and updating cart count
     */
    const AddToBasketHandler = {
        basketCountSelector: '.js-cart-items-count, .header__cart-count',
        addCartButtonClass: '.js-add-cart', // ✅ Changed to match button class
        isProcessing: false,

        init() {
            console.log('✅ AddToBasketHandler initialized');
            this.attachEventListeners();
            this.updateBasketCount();
        },

        attachEventListeners() {
            // Use jQuery event delegation for better compatibility
            $(document).on('click', this.addCartButtonClass, (e) => {
                e.preventDefault();
                e.stopPropagation();

                if (this.isProcessing) {
                    console.log('⚠️ Already processing, please wait');
                    return;
                }

                const button = e.currentTarget;
                this.handleAddToBasket(button);
            });

            console.log('📌 Event listeners attached to:', this.addCartButtonClass);
        },

        handleAddToBasket(button) {
            const $button = $(button);
            const productVariantId = $button.data('variant-id') || $button.attr('data-variant-id');

            console.log('🛒 Add to basket clicked, Variant ID:', productVariantId);

            if (!productVariantId) {
                console.error('❌ Product variant ID not found');
                alert('Product variant not found. Please try again.');
                return;
            }

            // Disable button to prevent double clicks
            this.isProcessing = true;
            $button.prop('disabled', true);
            const originalText = $button.text();
            $button.text('Adding...'); // ✅ Show loading state

            console.log('🔄 Starting add to basket request...');

            this.addToBasket(parseInt(productVariantId), 1)
                .finally(() => {
                    this.isProcessing = false;
                    $button.prop('disabled', false);
                    $button.text(originalText);
                    console.log('✅ Button re-enabled');
                });
        },

        async addToBasket(productVariantId, quantity) {
            console.log(`📤 Sending request: Variant ID=${productVariantId}, Quantity=${quantity}`);

            const formData = new FormData();
            formData.append('productVariantId', productVariantId);
            formData.append('quantity', quantity);

            try {
                const response = await fetch('/Basket/Add', {
                    method: 'POST',
                    body: formData
                });

                console.log('📥 Response status:', response.status);

                if (!response.ok) {
                    throw new Error(`HTTP error! status: ${response.status}`);
                }

                const data = await response.json();
                console.log('📦 Response data:', data);

                if (data.success) {
                    // Update basket count immediately from the response
                    this.updateBasketCountFromData(data.totalCount);
                    this.showAddedNotification(data.message || 'Product added to basket!');
                    console.log('✅ Product added successfully, Total Count:', data.totalCount);
                } else {
                    console.error('❌ Failed to add item:', data.message);
                    alert('Failed to add item to basket. Please try again.');
                }
            } catch (error) {
                console.error('❌ Error adding to basket:', error);
                alert('An error occurred. Please try again.');
            }
        },

        updateBasketCount() {
            console.log('🔄 Updating basket count...');

            $.get('/Basket/GetBasket')
                .done((data) => {
                    console.log('📦 Basket data:', data);
                    if (data && data.totalCount !== undefined) {
                        this.updateBasketCountFromData(data.totalCount);
                    }
                })
                .fail((error) => {
                    console.error('❌ Error updating basket count:', error);
                });
        },

        updateBasketCountFromData(count) {
            console.log('🔢 Updating count to:', count);

            $(this.basketCountSelector).each(function () {
                const $el = $(this);
                $el.text(count);

                // Force display update
                if (count > 0) {
                    $el.show().css('display', 'inline-block');
                } else {
                    $el.hide();
                }
            });

            // ✅ Trigger a custom event that other scripts can listen to
            $(document).trigger('basketUpdated', [count]);
        },

        showAddedNotification(message) {
            console.log('🎉 Showing notification:', message);

            // Remove any existing toasts
            $('.basket-toast').remove();

            // Create a simple toast notification
            const $toast = $('<div>')
                .addClass('basket-toast')
                .text(message)
                .css({
                    'position': 'fixed',
                    'top': '20px',
                    'right': '20px',
                    'background': '#28a745',
                    'color': 'white',
                    'padding': '15px 25px',
                    'border-radius': '5px',
                    'z-index': '10000',
                    'box-shadow': '0 4px 6px rgba(0,0,0,0.1)',
                    'animation': 'slideIn 0.3s ease-out'
                });

            $('body').append($toast);

            setTimeout(() => {
                $toast.css('animation', 'slideOut 0.3s ease-out');
                setTimeout(() => $toast.remove(), 300);
            }, 2000);
        }
    };

    // Add CSS animations
    if (!document.getElementById('basket-animations')) {
        const style = document.createElement('style');
        style.id = 'basket-animations';
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
    }

    // Initialize when DOM and jQuery are ready
    $(document).ready(function () {
        console.log('📄 DOM ready, initializing...');
        AddToBasketHandler.init();
    });
})();