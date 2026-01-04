(function () {
    'use strict';

    // Quick View Modal Handler - updated to use inline data attributes (no fetch)
    const quickViewModalEl = document.getElementById('quickViewModal'); // matches Views/Shared/_QuickViewModal.cshtml
    let currentProductId = null;
    let currentProductData = null;
    let isAddingToCart = false;
    let isAddingToWishlist = false;
    let isRemovingFromCart = false;

    // Event delegation for quick view buttons (uses existing data-* attributes rendered by _ProductCard.cshtml)
    document.addEventListener('click', function (e) {
        const btn = e.target.closest('.js-quick-view, .pc__quick-view');
        if (!btn) return;

        e.preventDefault();

        const productCard = btn.closest('.product-card-wrapper, .product-card');
        if (!productCard) {
            showNotification('Could not locate product card. Please try again.', 'danger');
            return;
        }

        const productId = btn.dataset.productId || productCard.dataset.productId;
        if (!productId) {
            showNotification('Could not load product. Please try again.', 'danger');
            return;
        }

        currentProductId = productId;

        // Read inline JSON variants + metadata set in _ProductCard.cshtml
        try {
            const variantsJson = (productCard.getAttribute('data-product-variants') || productCard.dataset.productVariants || '').trim();
            const variants = variantsJson ? JSON.parse(variantsJson) : [];
            currentProductData = {
                id: productId,
                name: productCard.dataset.productName || productCard.querySelector('.pc__title a')?.textContent?.trim() || 'Product',
                brandName: productCard.dataset.productBrand || productCard.querySelector('.pc__category')?.textContent?.trim() || '',
                detailsUrl: (productCard.querySelector('a[href*="/Product/Details/"]') || {}).href || productCard.dataset.productUrl || productCard.querySelector('a')?.getAttribute('href') || '#',
                variants: variants
            };
        } catch (err) {
            console.error('Failed to parse product variants JSON:', err);
            currentProductData = {
                id: productId,
                name: productCard.dataset.productName || productCard.querySelector('.pc__title a')?.textContent?.trim() || 'Product',
                brandName: productCard.dataset.productBrand || '',
                detailsUrl: productCard.querySelector('a')?.getAttribute('href') || '#',
                variants: []
            };
        }

        populateQuickView(currentProductData);

        // Show Bootstrap modal (matches id in _QuickViewModal.cshtml)
        if (quickViewModalEl) {
            const modal = new bootstrap.Modal(quickViewModalEl);
            modal.show();
        }
    });

    function populateQuickView(data) {
        // Elements in _QuickViewModal.cshtml
        const qvTitle = document.getElementById('qvTitle');
        const qvBrand = document.getElementById('qvBrand');
        const qvMainImage = document.getElementById('qvMainImage');
        const qvSalePrice = document.getElementById('qvSalePrice');
        const qvOriginalPrice = document.getElementById('qvOriginalPrice');
        const qvOldPrice = document.getElementById('qvOldPrice');
        const qvStockStatus = document.getElementById('qvStockStatus');
        const qvQuantity = document.getElementById('qvQuantity');
        const qvViewDetails = document.getElementById('qvViewDetails');
        const qvColorSection = document.getElementById('qvColorSection');
        const qvColorSwatches = document.getElementById('qvColorSwatches');
        const qvSelectedColorName = document.getElementById('qvSelectedColorName');
        const qvSizeSection = document.getElementById('qvSizeSection');
        const qvSizeOptions = document.getElementById('qvSizeOptions');

        if (qvTitle) qvTitle.textContent = data.name || 'Product';
        if (qvBrand) qvBrand.textContent = data.brandName || '';
        if (qvViewDetails) qvViewDetails.setAttribute('href', data.detailsUrl || '#');
        if (qvQuantity) qvQuantity.value = 1;

        // Pick a primary variant (best-effort)
        const v = (data.variants && data.variants.length) ? data.variants[0] : null;

        // Image
        if (qvMainImage) {
            if (v && (v.coverImageName || (v.imageNames && v.imageNames.length))) {
                const filename = v.coverImageName || (v.imageNames && v.imageNames[0]) || 'product-placeholder.jpg';
                qvMainImage.src = filename.includes('/') ? filename : `/images/products/${filename}`;
                qvMainImage.alt = data.name || 'Product Image';
            } else {
                // fallback to first <img> found on card or placeholder
                const cardImg = document.querySelector(`.product-card-wrapper[data-product-id="${data.id}"] img.main-product-image`);
                qvMainImage.src = cardImg ? cardImg.src : '/images/products/product-placeholder.jpg';
            }
        }

        // Prices
        const currencySymbol = window.__currencySymbol || '$'; // optional global
        if (v && v.salePrice && v.salePrice < v.price) {
            if (qvSalePrice) { qvSalePrice.textContent = `${currencySymbol}${parseFloat(v.salePrice).toFixed(2)}`; qvSalePrice.classList.remove('d-none'); }
            if (qvOldPrice) { qvOldPrice.textContent = `${currencySymbol}${parseFloat(v.price).toFixed(2)}`; qvOldPrice.classList.remove('d-none'); }
            if (qvOriginalPrice) qvOriginalPrice.classList.add('d-none');
        } else {
            if (qvOriginalPrice) { qvOriginalPrice.textContent = `${currencySymbol}${v ? parseFloat(v.price || 0).toFixed(2) : '0.00'}`; qvOriginalPrice.classList.remove('d-none'); }
            if (qvSalePrice) qvSalePrice.classList.add('d-none');
            if (qvOldPrice) qvOldPrice.classList.add('d-none');
        }

        // Stock
        if (qvStockStatus) {
            const inStock = v ? (v.quantity > 0) : true;
            qvStockStatus.innerHTML = inStock ? `<span class="badge bg-success-line">In Stock</span>` : `<span class="badge bg-danger-line">Out of Stock</span>`;
        }

        // Colors
        if (v && data.variants && data.variants.length) {
            const uniqueColors = [];
            const seen = new Set();
            data.variants.forEach(variant => {
                if (variant.colorId || variant.colorHexCode || variant.colorName) {
                    const key = variant.colorId || variant.colorHexCode || variant.colorName;
                    if (!seen.has(key)) { seen.add(key); uniqueColors.push(variant); }
                }
            });

            if (uniqueColors.length > 0) {
                if (qvColorSection) qvColorSection.classList.remove('d-none');
                qvColorSwatches.innerHTML = '';
                uniqueColors.forEach((c, idx) => {
                    const sw = document.createElement('div');
                    sw.className = 'qv-color-swatch' + (idx === 0 ? ' active' : '');
                    sw.style.backgroundColor = c.colorHexCode || '#ccc';
                    sw.title = c.colorName || '';
                    sw.dataset.colorId = c.colorId || '';
                    sw.dataset.colorName = c.colorName || '';
                    sw.dataset.variantId = c.id || '';
                    sw.addEventListener('click', function () {
                        // select color and update main image/price/stock quickly
                        qvSelectedColorName && (qvSelectedColorName.textContent = c.colorName || '');
                        document.querySelectorAll('.qv-color-swatch').forEach(el => el.classList.remove('active'));
                        sw.classList.add('active');
                        // find first variant matching this color
                        const matched = data.variants.find(x => String(x.colorId) === String(c.colorId) || x.id === c.id) || c;
                        // update image
                        if (qvMainImage) {
                            const imgFile = matched.coverImageName || (matched.imageNames && matched.imageNames[0]) || qvMainImage.src;
                            qvMainImage.src = imgFile.includes('/') ? imgFile : `/images/products/${imgFile}`;
                        }
                        // update price & stock
                        if (matched.salePrice && matched.salePrice < matched.price) {
                            qvSalePrice && (qvSalePrice.textContent = `${currencySymbol}${parseFloat(matched.salePrice).toFixed(2)}`, qvSalePrice.classList.remove('d-none'));
                            qvOldPrice && (qvOldPrice.textContent = `${currencySymbol}${parseFloat(matched.price).toFixed(2)}`, qvOldPrice.classList.remove('d-none'));
                            qvOriginalPrice && qvOriginalPrice.classList.add('d-none');
                        } else {
                            qvOriginalPrice && (qvOriginalPrice.textContent = `${currencySymbol}${parseFloat(matched.price || 0).toFixed(2)}`, qvOriginalPrice.classList.remove('d-none'));
                            qvSalePrice && qvSalePrice.classList.add('d-none');
                            qvOldPrice && qvOldPrice.classList.add('d-none');
                        }
                        qvStockStatus && (qvStockStatus.innerHTML = (matched.quantity > 0) ? `<span class="badge bg-success-line">In Stock</span>` : `<span class="badge bg-danger-line">Out of Stock</span>`);
                        // update view details link to include colorId
                        if (qvViewDetails) {
                            let url = data.detailsUrl || '#';
                            url += (url.indexOf('?') === -1 ? '?' : '&') + `colorId=${encodeURIComponent(matched.colorId || '')}`;
                            qvViewDetails.setAttribute('href', url);
                        }
                    });
                    qvColorSwatches.appendChild(sw);
                    if (idx === 0) qvSelectedColorName && (qvSelectedColorName.textContent = c.colorName || '');
                });
            } else {
                if (qvColorSection) qvColorSection.classList.add('d-none');
            }

            // Sizes
            const availableVariants = data.variants.filter(variant => {
                const activeColor = document.querySelector('.qv-color-swatch.active')?.dataset.colorId;
                return !activeColor || String(variant.colorId) === String(activeColor);
            });
            const sizes = Array.from(new Set(availableVariants.map(x => x.size).filter(Boolean)));
            if (sizes.length > 0) {
                if (qvSizeSection) qvSizeSection.classList.remove('d-none');
                qvSizeOptions.innerHTML = '';
                sizes.forEach((s, i) => {
                    const el = document.createElement('div');
                    el.className = 'qv-size-option' + (i === 0 ? ' active' : '');
                    el.textContent = s;
                    el.dataset.size = s;
                    el.addEventListener('click', function () {
                        document.querySelectorAll('.qv-size-option').forEach(x => x.classList.remove('active'));
                        el.classList.add('active');
                        // choose variant with color/size and update price/stock/image like color click above
                        const activeColor = document.querySelector('.qv-color-swatch.active')?.dataset.colorId;
                        const matched = data.variants.find(x => (!activeColor || String(x.colorId) === String(activeColor)) && x.size === s) || data.variants[0];
                        if (matched) {
                            if (qvMainImage) {
                                const imgFile = matched.coverImageName || (matched.imageNames && matched.imageNames[0]) || qvMainImage.src;
                                qvMainImage.src = imgFile.includes('/') ? imgFile : `/images/products/${imgFile}`;
                            }
                            if (matched.salePrice && matched.salePrice < matched.price) {
                                qvSalePrice && (qvSalePrice.textContent = `${currencySymbol}${parseFloat(matched.salePrice).toFixed(2)}`, qvSalePrice.classList.remove('d-none'));
                                qvOldPrice && (qvOldPrice.textContent = `${currencySymbol}${parseFloat(matched.price).toFixed(2)}`, qvOldPrice.classList.remove('d-none'));
                                qvOriginalPrice && qvOriginalPrice.classList.add('d-none');
                            } else {
                                qvOriginalPrice && (qvOriginalPrice.textContent = `${currencySymbol}${parseFloat(matched.price || 0).toFixed(2)}`, qvOriginalPrice.classList.remove('d-none'));
                                qvSalePrice && qvSalePrice.classList.add('d-none');
                                qvOldPrice && qvOldPrice.classList.add('d-none');
                            }
                            qvStockStatus && (qvStockStatus.innerHTML = (matched.quantity > 0) ? `<span class="badge bg-success-line">In Stock</span>` : `<span class="badge bg-danger-line">Out of Stock</span>`);
                        }
                    });
                    qvSizeOptions.appendChild(el);
                });
            } else {
                if (qvSizeSection) qvSizeSection.classList.add('d-none');
            }
        }

        // Reset buttons UI
        resetButtonStates();
    }

    // Add to cart from quick view
    document.addEventListener('click', function (e) {
        const btn = e.target.closest('#qvAddToCart, #qvAddToCart');
        if (!btn) return;
        e.preventDefault();

        if (isAddingToCart) return;

        // Determine selected variant id
        let selectedVariant = null;
        const activeColorId = document.querySelector('.qv-color-swatch.active')?.dataset.variantId;
        const selectedSize = document.querySelector('.qv-size-option.active')?.dataset.size;
        if (currentProductData && currentProductData.variants && currentProductData.variants.length) {
            selectedVariant = currentProductData.variants.find(v =>
                (activeColorId ? String(v.id) === String(activeColorId) : true) &&
                (selectedSize ? String(v.size) === String(selectedSize) : true)
            ) || currentProductData.variants[0];
        }

        const quantityEl = document.getElementById('qvQuantity');
        const quantity = quantityEl ? parseInt(quantityEl.value, 10) || 1 : 1;

        if (!selectedVariant) {
            showNotification('Unable to identify product variant. Please try again.', 'danger');
            return;
        }

        addToCart(selectedVariant.id || selectedVariant.productVariantId || selectedVariant.productId || currentProductId, quantity);
    });

    function addToCart(productVariantId, quantity) {
        isAddingToCart = true;
        const btn = document.getElementById('qvAddToCart');
        const originalHtml = btn ? btn.innerHTML : '';

        if (btn) {
            btn.disabled = true;
            btn.innerHTML = '<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>Adding...';
        }

        const formData = new FormData();
        formData.append('productVariantId', productVariantId);
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
                updateCartCount();
                showNotification(`${quantity} item(s) added to basket!`, 'success');
                // close modal
                if (quickViewModalEl) {
                    const modal = bootstrap.Modal.getInstance(quickViewModalEl);
                    if (modal) setTimeout(() => modal.hide(), 300);
                }
            })
            .catch(error => {
                console.error('Add to cart error:', error);
                showNotification('Unable to add product to basket. Please try again.', 'danger');
            })
            .finally(() => {
                isAddingToCart = false;
                if (btn) {
                    btn.disabled = false;
                    btn.innerHTML = originalHtml;
                }
            });
    }

    // Update cart count in header
    function updateCartCount() {
        fetch('/Basket/GetBasket')
            .then(response => response.json())
            .then(data => {
                const cartCountEls = document.querySelectorAll('[data-cart-count], .js-cart-items-count, .cart-amount');
                cartCountEls.forEach(el => {
                    el.textContent = data.count || data.totalCount || 0;
                    if (data.totalCount > 0) el.style.display = '';
                });
            })
            .catch(err => console.log('Could not update cart count:', err));
    }

    // Wishlist (simple POST)
    document.addEventListener('click', function (e) {
        const btn = e.target.closest('#qvAddToWishlist, #qvAddWishlist');
        if (!btn) return;
        e.preventDefault();
        if (isAddingToWishlist) return;

        isAddingToWishlist = true;
        btn.disabled = true;

        fetch('/Wishlist/Add', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' },
            body: `id=${encodeURIComponent(currentProductId)}`
        })
            .then(resp => {
                if (resp.status === 204 || resp.ok) {
                    showNotification('Added to wishlist!', 'success');
                } else if (resp.status === 401) {
                    showNotification('Please login to add to wishlist.', 'warning');
                    setTimeout(() => window.location.href = '/Account/Login?returnUrl=' + encodeURIComponent(window.location.pathname), 1200);
                } else {
                    throw new Error('Wishlist add failed');
                }
            })
            .catch(err => {
                console.error('Wishlist error:', err);
                showNotification('Unable to add to wishlist. Please try again.', 'danger');
            })
            .finally(() => {
                isAddingToWishlist = false;
                btn.disabled = false;
            });
    });

    // Reset button states when modal opens
    function resetButtonStates() {
        const addBtn = document.getElementById('qvAddToCart');
        const wishlistBtn = document.getElementById('qvAddToWishlist');
        if (addBtn) { addBtn.disabled = false; addBtn.innerHTML = '<i class="fa fa-shopping-cart me-2"></i>Add To Cart'; }
        if (wishlistBtn) { wishlistBtn.disabled = false; }
    }

    // Simple notification
    function showNotification(message, type = 'success') {
        const alertDiv = document.createElement('div');
        const alertClass = type === 'danger' ? 'alert-danger' : (type === 'warning' ? 'alert-warning' : 'alert-success');
        alertDiv.className = `alert ${alertClass} position-fixed top-0 start-50 translate-middle-x mt-3`;
        alertDiv.style.zIndex = '9999';
        alertDiv.setAttribute('role', 'alert');
        alertDiv.textContent = message;
        document.body.appendChild(alertDiv);

        setTimeout(() => {
            alertDiv.classList.add('fade');
            setTimeout(() => alertDiv.remove(), 150);
        }, 2500);
    }
})();