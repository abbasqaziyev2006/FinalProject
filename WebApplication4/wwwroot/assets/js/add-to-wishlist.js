// Add to Wishlist Functionality
(function () {
    'use strict';

    console.log('Wishlist module loaded');

    // Handle wishlist button clicks (both add and remove)
    document.addEventListener('click', async function (e) {
        const wishlistBtn = e.target.closest('.js-add-wishlist');
        if (!wishlistBtn) return;

        e.preventDefault();
        e.stopPropagation();

        const productId = wishlistBtn.dataset.productId;
        if (!productId) {
            console.error('Product ID not found on button:', wishlistBtn);
            return;
        }

        const isActive = wishlistBtn.classList.contains('active');
        const isOnWishlistPage = window.location.pathname.toLowerCase().includes('/wishlist');

        console.log('Wishlist button clicked:', { productId, isActive, isOnWishlistPage });

        // Disable button during request
        wishlistBtn.style.pointerEvents = 'none';
        wishlistBtn.style.opacity = '0.6';

        try {
            if (isActive) {
                // Remove from wishlist
                console.log('Removing from wishlist:', productId);
                const response = await fetch(`/Wishlist/Remove/${productId}`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    }
                });

                console.log('Remove response:', response.status, response.statusText);

                if (response.ok) {
                    wishlistBtn.classList.remove('active');

                    // If on wishlist page, remove the entire product card
                    if (isOnWishlistPage) {
                        const card = wishlistBtn.closest('.product-card-wrapper');
                        if (card) {
                            // Animate and remove
                            card.style.transition = 'all 0.3s ease';
                            card.style.opacity = '0';
                            card.style.transform = 'scale(0.8)';

                            setTimeout(() => {
                                card.remove();

                                // Check if any products remain
                                const remainingCards = document.querySelectorAll('.product-card-wrapper');
                                if (remainingCards.length === 0) {
                                    // Reload to show empty state
                                    location.reload();
                                }
                            }, 300);
                        }
                    }

                    showNotification('Removed from wishlist', 'success');
                    updateWishlistCount();
                } else if (response.status === 401) {
                    window.location.href = '/Account/Login?returnUrl=' + encodeURIComponent(window.location.pathname);
                } else {
                    const errorText = await response.text();
                    console.error('Server error:', errorText);
                    throw new Error('Failed to remove from wishlist');
                }
            } else {
                // Add to wishlist
                console.log('Adding to wishlist:', productId);
                const response = await fetch('/Wishlist/Add', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/x-www-form-urlencoded',
                    },
                    body: `id=${productId}`
                });

                console.log('Add response:', response.status, response.statusText);

                if (response.ok) {
                    wishlistBtn.classList.add('active');
                    showNotification('Added to wishlist', 'success');
                    updateWishlistCount();
                } else if (response.status === 401) {
                    window.location.href = '/Account/Login?returnUrl=' + encodeURIComponent(window.location.pathname);
                } else {
                    const errorText = await response.text();
                    console.error('Server error:', errorText);
                    throw new Error('Failed to add to wishlist');
                }
            }
        } catch (error) {
            console.error('Wishlist error:', error);
            showNotification('An error occurred. Please try again.', 'error');
        } finally {
            // Re-enable button
            wishlistBtn.style.pointerEvents = '';
            wishlistBtn.style.opacity = '';
        }
    });

    // Update wishlist count in header
    async function updateWishlistCount() {
        try {
            const response = await fetch('/Wishlist/GetWishlistJ');
            if (response.ok) {
                const data = await response.json();
                console.log('Wishlist count updated:', data.count);
                const countElements = document.querySelectorAll('.header__wishlist-count, .js-wishlist-count');
                countElements.forEach(el => {
                    el.textContent = data.count || 0;
                    el.style.display = data.count > 0 ? '' : 'none';
                });
            }
        } catch (error) {
            console.error('Failed to update wishlist count:', error);
        }
    }

    // Show notification
    function showNotification(message, type = 'success') {
        const existing = document.querySelector('.wishlist-notification');
        if (existing) {
            existing.remove();
        }

        const notification = document.createElement('div');
        notification.className = `wishlist-notification alert alert-${type === 'success' ? 'success' : 'danger'} position-fixed`;
        notification.style.cssText = 'top: 100px; right: 20px; z-index: 9999; min-width: 250px; animation: slideInRight 0.3s ease;';
        notification.innerHTML = `
            <div class="d-flex align-items-center">
                <span>${message}</span>
            </div>
        `;

        document.body.appendChild(notification);

        setTimeout(() => {
            notification.style.animation = 'slideOutRight 0.3s ease';
            setTimeout(() => notification.remove(), 300);
        }, 3000);
    }

    // Add CSS animations
    const style = document.createElement('style');
    style.textContent = `
        @keyframes slideInRight {
            from {
                transform: translateX(100%);
                opacity: 0;
            }
            to {
                transform: translateX(0);
                opacity: 1;
            }
        }
        @keyframes slideOutRight {
            from {
                transform: translateX(0);
                opacity: 1;
            }
            to {
                transform: translateX(100%);
                opacity: 0;
            }
        }
        .wishlist-notification {
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
        }
        .js-add-wishlist.active svg {
            fill: #ff6b6b !important;
        }
        .js-add-wishlist:not(.active) svg {
            fill: none !important;
        }
        .js-add-wishlist {
            transition: transform 0.2s ease;
            cursor: pointer;
        }
        .js-add-wishlist:hover {
            transform: scale(1.1);
        }
    `;
    document.head.appendChild(style);

    // Initialize on page load - mark already wishlisted products
    document.addEventListener('DOMContentLoaded', async function () {
        console.log('Initializing wishlist state...');
        try {
            const response = await fetch('/Wishlist/GetWishlistJ');
            if (response.ok) {
                const data = await response.json();
                console.log('Current wishlist:', data);
                if (data.items && data.items.length > 0) {
                    const wishlistedIds = data.items.map(item => item.productId);
                    console.log('Wishlisted product IDs:', wishlistedIds);

                    // Mark wishlist buttons as active
                    document.querySelectorAll('.js-add-wishlist').forEach(btn => {
                        const productId = parseInt(btn.dataset.productId);
                        if (wishlistedIds.includes(productId)) {
                            btn.classList.add('active');
                            console.log('Marked product as wishlisted:', productId);
                        }
                    });

                    // Update header count
                    updateWishlistCount();
                }
            }
        } catch (error) {
            console.log('Wishlist initialization skipped (user may not be logged in)', error);
        }
    });
})();