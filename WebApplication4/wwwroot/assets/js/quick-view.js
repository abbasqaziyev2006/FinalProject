(function () {
    'use strict';

    // Quick View Modal Handler
    const quickViewModal = document.getElementById('quickView');
    let currentProductId = null;
    let isAddingToCart = false;
    let isAddingToWishlist = false;

    // Event delegation for quick view buttons
    document.addEventListener('click', function (e) {
        if (e.target.classList.contains('js-quick-view')) {
            e.preventDefault();
            const productCard = e.target.closest('.product-card, .product-card-wrapper, .swiper-slide');

            if (productCard) {
                let productId = e.target.dataset.productId;

                if (!productId) {
                    // Try to get ID from parent product card's data attribute
                    productId = productCard.dataset.productId;
                }

                if (!productId) {
                    // Fallback: extract from link href
                    const link = productCard.querySelector('a[href*="/Product/Details/"]');
                    if (link) {
                        // Extract ID from URL like /Product/Details/123-product-name
                        const matches = link.href.match(/\/Product\/Details\/(\d+)/);
                        if (matches) {
                            productId = matches[1];
                        }
                    }
                }

                if (productId) {
                    loadQuickView(productId);
                } else {
                    showNotification('Could not load product. Please try again.', 'danger');
                }
            }
        }
    });

    function loadQuickView(productId) {
        currentProductId = productId;

        // Construct the details URL with the product ID
        const detailsUrl = `/Product/Details/${productId}-product`;

        fetch(detailsUrl)
            .then(response => {
                if (!response.ok) throw new Error('Product not found');
                return response.text();
            })
            .then(html => {
                // Parse the HTML response to extract product details
                const parser = new DOMParser();
                const doc = parser.parseFromString(html, 'text/html');

                // Extract product data from the details page
                const product = extractProductData(doc, html);
                populateQuickView(product);
            })
            .catch(error => {
                console.error('Error loading quick view:', error);
                showNotification('Unable to load product details. Please try again.', 'danger');
            });
    }

    function extractProductData(doc, html) {
        // Try multiple selectors to find product information
        const name =
            doc.querySelector('h1')?.textContent?.trim() ||
            doc.querySelector('.product-title')?.textContent?.trim() ||
            doc.querySelector('.page-title')?.textContent?.trim() ||
            'Product Title';

        const category =
            doc.querySelector('[data-category]')?.textContent?.trim() ||
            doc.querySelector('.category')?.textContent?.trim() ||
            'Uncategorized';

        let imageUrl = '/assets/images/placeholder.jpg';
        const imgEl = doc.querySelector('img.product-img, img.pc__img, .product-image img, [data-product-image]');
        if (imgEl) {
            imageUrl = imgEl.src || imgEl.dataset.src || imageUrl;
        }

        let price = 0;
        let salePrice = 0;

        // Try to extract price from common selectors
        const priceEl = doc.querySelector('[data-price], .product-price, .price, .money');
        if (priceEl) {
            price = parseFloat(priceEl.textContent.replace(/[^\d.]/g, '')) || 0;
        }

        const salePriceEl = doc.querySelector('[data-sale-price], .product-sale-price, .sale-price, .price-sale');
        if (salePriceEl) {
            salePrice = parseFloat(salePriceEl.textContent.replace(/[^\d.]/g, '')) || 0;
        }

        const description =
            doc.querySelector('[data-description]')?.textContent?.trim() ||
            doc.querySelector('.product-description')?.textContent?.trim() ||
            doc.querySelector('.description')?.textContent?.trim() ||
            'No description available';

        let rating = 0;
        let reviewCount = 0;
        const ratingEl = doc.querySelector('[data-rating]');
        if (ratingEl) {
            rating = parseInt(ratingEl.dataset.rating || 0);
            reviewCount = parseInt(ratingEl.dataset.reviewCount || 0);
        }

        return {
            name,
            category,
            imageUrl,
            price,
            salePrice,
            description,
            rating,
            reviewCount
        };
    }

    function populateQuickView(product) {
        const qvTitle = document.getElementById('qvTitle');
        const qvCategory = document.getElementById('qvCategory');
        const qvImage = document.getElementById('qvImage');
        const qvPrice = document.getElementById('qvPrice');
        const qvPriceSale = document.getElementById('qvPriceSale');
        const qvDescription = document.getElementById('qvDescription');
        const qvQuantity = document.getElementById('qvQuantity');
        const qvReviews = document.getElementById('qvReviews');

        if (qvTitle) qvTitle.textContent = product.name;
        if (qvCategory) qvCategory.textContent = product.category || 'Uncategorized';
        if (qvImage) {
            qvImage.src = product.imageUrl;
            qvImage.alt = product.name;
        }
        if (qvPrice) qvPrice.textContent = `$${parseFloat(product.price).toFixed(2)}`;
        if (qvDescription) qvDescription.textContent = product.description || 'No description available';
        if (qvQuantity) qvQuantity.value = 1;

        // Handle sale price
        if (qvPriceSale) {
            if (product.salePrice && product.salePrice < product.price) {
                qvPriceSale.textContent = `$${parseFloat(product.salePrice).toFixed(2)}`;
                qvPriceSale.classList.remove('d-none');
            } else {
                qvPriceSale.classList.add('d-none');
            }
        }

        // Populate reviews if available
        if (qvReviews) {
            if (product.rating !== undefined && product.rating > 0) {
                qvReviews.innerHTML = `
                    <div class="product-card__review d-flex align-items-center">
                        <div class="reviews-group d-flex">
                            ${generateStarRating(product.rating)}
                        </div>
                        <span class="reviews-note text-lowercase text-secondary ms-1">${product.reviewCount || 0} reviews</span>
                    </div>
                `;
            } else {
                qvReviews.innerHTML = '';
            }
        }

        // Reset button states
        resetButtonStates();
    }

    function generateStarRating(rating) {
        let stars = '';
        const fullRating = Math.round(rating);
        for (let i = 0; i < 5; i++) {
            stars += `<svg class="review-star ${i < fullRating ? 'active' : ''}" viewBox="0 0 9 9" xmlns="http://www.w3.org/2000/svg">
                        <use href="#icon_star" />
                      </svg>`;
        }
        return stars;
    }

    // Add to cart from quick view
    const addCartBtn = document.getElementById('qvAddCart');
    if (addCartBtn) {
        addCartBtn.addEventListener('click', function () {
            const qvQuantity = document.getElementById('qvQuantity');
            const quantity = qvQuantity ? parseInt(qvQuantity.value) : 1;

            if (quantity < 1 || isNaN(quantity)) {
                showNotification('Please enter a valid quantity', 'danger');
                return;
            }

            if (isAddingToCart) return;

            addToCart(currentProductId, quantity);
        });
    }

    // Add to wishlist from quick view
    const addWishlistBtn = document.getElementById('qvAddWishlist');
    if (addWishlistBtn) {
        addWishlistBtn.addEventListener('click', function () {
            if (isAddingToWishlist) return;
            addToWishlist(currentProductId);
        });
    }

    // Generic Add to Cart function - matches your /Basket/Add endpoint
    function addToCart(productId, quantity) {
        isAddingToCart = true;
        const btn = document.getElementById('qvAddCart');
        const originalText = btn.textContent;
        btn.disabled = true;
        btn.innerHTML = '<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>Adding...';

        const formData = new FormData();
        formData.append('productVariantId', productId);
        formData.append('quantity', quantity);

        fetch('/Basket/Add', {
            method: 'POST',
            body: formData
        })
            .then(response => {
                if (!response.ok) throw new Error('Failed to add to cart');
                return response.json();
            })
            .then(data => {
                console.log('Added to cart:', data);
                updateCartCount();
                showNotification(`${quantity} item(s) added to basket!`, 'success');

                // Close modal after successful add
                if (quickViewModal) {
                    const modal = bootstrap.Modal.getInstance(quickViewModal);
                    if (modal) {
                        setTimeout(() => modal.hide(), 500);
                    }
                }
            })
            .catch(error => {
                console.error('Error adding to cart:', error);
                showNotification('Unable to add product to basket. Please try again.', 'danger');
            })
            .finally(() => {
                isAddingToCart = false;
                btn.disabled = false;
                btn.textContent = originalText;
            });
    }

    // Generic Add to Wishlist function
    function addToWishlist(productId) {
        isAddingToWishlist = true;
        const btn = document.getElementById('qvAddWishlist');
        btn.disabled = true;

        fetch('/api/wishlist/add', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                productId: productId
            })
        })
            .then(response => {
                if (!response.ok) throw new Error('Failed to add to wishlist');
                return response.json();
            })
            .then(data => {
                btn.classList.toggle('active');
                showNotification('Added to wishlist!', 'success');
                updateWishlistCount();
            })
            .catch(error => {
                console.error('Error adding to wishlist:', error);
                showNotification('Unable to add to wishlist. Please try again.', 'danger');
            })
            .finally(() => {
                isAddingToWishlist = false;
                btn.disabled = false;
            });
    }

    // Update cart count in header
    function updateCartCount() {
        fetch('/Basket/GetBasket')
            .then(response => response.json())
            .then(data => {
                const cartCountEls = document.querySelectorAll('[data-cart-count]');
                cartCountEls.forEach(el => {
                    el.textContent = data.count || data.totalCount || 0;
                });
            })
            .catch(error => console.log('Could not update cart count:', error));
    }

    // Update wishlist count in header
    function updateWishlistCount() {
        fetch('/api/wishlist/count')
            .then(response => response.json())
            .then(data => {
                const wishlistCountEls = document.querySelectorAll('[data-wishlist-count]');
                wishlistCountEls.forEach(el => {
                    el.textContent = data.count || 0;
                });
            })
            .catch(error => console.log('Could not update wishlist count:', error));
    }

    // Reset button states when modal opens
    function resetButtonStates() {
        const addCartBtn = document.getElementById('qvAddCart');
        const addWishlistBtn = document.getElementById('qvAddWishlist');

        if (addCartBtn) {
            addCartBtn.disabled = false;
            addCartBtn.textContent = 'Add To Basket';
        }
        if (addWishlistBtn) {
            addWishlistBtn.disabled = false;
        }
    }

    // Notification helper with type support
    function showNotification(message, type = 'success') {
        const alertDiv = document.createElement('div');
        const alertClass = type === 'danger' ? 'alert-danger' : 'alert-success';
        alertDiv.className = `alert ${alertClass} position-fixed top-0 start-50 translate-middle-x mt-3`;
        alertDiv.style.zIndex = '9999';
        alertDiv.setAttribute('role', 'alert');
        alertDiv.textContent = message;
        document.body.appendChild(alertDiv);

        setTimeout(() => {
            alertDiv.classList.add('fade');
            setTimeout(() => alertDiv.remove(), 150);
        }, 3000);
    }
})();