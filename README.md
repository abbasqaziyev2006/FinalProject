# 🛍️ Full Featured E-Commerce Web Application

This project is a **full-featured e-commerce web application** developed with **ASP.NET MVC**.  
The main goal of the project was to simulate a real-world online shopping system with both **customer-side** and **admin-side** functionalities.

The application includes advanced shopping features, user account management, order processing, payment integration, and a dynamic admin panel.

---

## 🔐 Authentication & User Experience

- Each user has a **separate basket stored per account**  
- Cart data **persists after logout and login**  
- Forgot Password & Contact forms **work with email**  
- Multiple login attempts trigger **temporary account blocking**  
- Language switching (multi-language support)  
- Currency switching  
- Dark Mode support  
- Fully responsive UI  

---

## 🛒 Shopping & Customer Features

- Product listing with:
  - Search
  - Sorting
  - Filtering
  - Category-based navigation
- Infinite scroll in:
  - Shop
  - Wishlist
- Wishlist system (per user)  
- Add to cart / remove from cart  
- Quick View modal for products  
- If user is not logged in, actions redirect to login page  
- Grid view options (2 / 3 / 4 columns)  
- Dynamic notifications for:
  - Add to cart
  - Add to wishlist
  - Remove from basket
- Smooth navigation (returning to previous scroll position)  
- Brand section with custom-added brand logos  

---

## ⭐ Reviews & Ratings

- Dynamic product rating system  
- Users can:
  - Add reviews
  - Edit their own reviews
  - Delete their own reviews
- “My Reviews” section for logged-in users  
- Ratings update dynamically based on reviews  

**Admin Review Management:**  
- Admin can **edit or delete any review**  
- Admin dashboard shows review summaries for each product  
- Allows moderation of user-submitted content  

---

## 💳 Orders & Payments

- Full order flow implemented  
- Stripe payment integration  
- Order statuses handled properly (pending, completed, cancelled)  
- Orders linked to user accounts  

---

## 🛠️ Admin Panel Features

### 👥 User Management
- Assign roles to users from admin panel  
- Automatic user blocking after multiple failed login attempts  

### 📊 Dashboard
- Fully dynamic dashboard  
- Displays:
  - Pending orders  
  - Cancelled orders  
  - Total revenue  
  - Successful orders  

### 🏷️ Product & Catalog Management
- Add / Edit / Delete products  
- Deactivate products (hidden from shop)  
- Bulk add for products:
  - Size
  - Color
  - Sale price  
- Discount management:
  - Percentage-based discounts
  - Automatically calculates discounted price  
- Category management  
- Slider management  
- Drag & drop support for:
  - Products
  - Categories
  - Sliders  

### ✍️ Content Management
- Special rich text editor for product descriptions  
- Footer managed via ViewModel  
- Custom locations and logos added dynamically  

---

## 🧩 UI & UX Details

- Infinite scroll implemented in multiple sections  
- Quick view modal  
- Custom-designed notifications  
- Dark mode  
- Clean and modern layout  
- Optimized shopping flow  

---

## 🛠️ Technologies Used

- C#  
- ASP.NET MVC  
- Entity Framework  
- SQL Server  
- Razor Views  
- HTML / CSS / JavaScript  
- Stripe API  

---

## 📁 Project Structure
