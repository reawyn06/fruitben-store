# FruitBen: Fruit E-Commerce Website

FruitBen is a web-based fruit store built as a final project for the **Web Programming** course at the Ho Chi Minh City University of Industry and Trade (HUIT), Faculty of Information Technology. It supports two user groups: **customers**, who browse, search, and order fruit online, and **administrators**, who manage products, orders, customers, discount codes, news, and sales reports.

## Table of Contents

- [Features](#features)
- [Tech Stack](#tech-stack)
- [System Overview](#system-overview)
- [Database Design](#database-design)
- [Getting Started](#getting-started)
- [Usage](#usage)
- [Business Rules](#business-rules)
- [Known Limitations](#known-limitations)
- [Future Work](#future-work)
- [Team](#team)
- [License](#license)

## Features

### Customer

- Home page with featured products, category cards, discount banners, and news highlights.
- Product catalog with category filtering (domestic or imported fruit), price range filtering, discounted-only filtering, keyword search, and sorting.
- Product detail page with image gallery, description, product information (category, price, stock, sold quantity, listing date, product code), storage guidelines, and related products.
- Shopping cart with quantity updates, item removal, and subtotal calculation.
- Checkout with recipient information, delivery address (province/city and ward/commune), and optional order notes.
- Discount code validation with a clear message when the order does not meet the code's conditions.
- Two payment methods: cash on delivery (COD) and bank transfer via QR code.
- Guest checkout without an account. Logged-in customers can view their full order history.
- Order cancellation, allowed only for the owner's orders in the *Pending* state.
- News section with article listing and detail pages.
- Contact form that stores messages in the database, plus support pages for payment, shipping, and returns.

### Administrator

- Dashboard with summary statistics, recent orders, low-stock alerts, and new customers.
- Product management: create, edit, delete, and stop selling products; upload images; filter by category and status.
- Order management: view details, move orders through the processing workflow (Pending, Shipping, Completed, Cancelled), and confirm bank transfer payments manually.
- Customer management: list accounts, search, and view each customer's orders, total spending, and latest order.
- Discount code management: create, edit, delete, and enable or disable codes with usage limits and validity periods.
- News management: create, edit, and delete articles with cover images.
- Sales statistics: total orders, total revenue, and revenue per product. Cancelled orders are excluded from revenue.

## Tech Stack

| Layer | Technology |
|---|---|
| Language | C# |
| Framework | ASP.NET MVC 5 (.NET Framework) |
| Data access | Entity Framework, LINQ, ADO.NET (used in the reporting module) |
| Database | Microsoft SQL Server |
| Views | Razor, HTML5, CSS3 |
| UI framework | Bootstrap |
| Client-side scripting | JavaScript, jQuery |
| IDE and tools | Visual Studio, SQL Server Management Studio, web browser dev tools |

## System Overview

The application follows the **Model-View-Controller (MVC)** pattern:

- **Model**: C# entity classes mapped to database tables through Entity Framework.
- **View**: Razor views that render HTML for customers and administrators.
- **Controller**: Handles requests, applies business logic (cart, checkout, discounts, order status), and returns views.

Access control separates customer and admin areas. Administrative functions are available only to accounts with the admin role.

## Database Design

The main tables are:

| Table | Purpose |
|---|---|
| `TaiKhoans` | User accounts (name, email, password, phone, role, registration date) |
| `SanPhams` | Products (name, category, price, old price, stock, image, description, status) |
| `HinhAnhSanPhams` | Additional product images (file name, display order) |
| `DonHangs` | Orders (recipient, contact, address, status, payment method, totals, discount, linked account) |
| `ChiTietDonHangs` | Order line items (order, product, quantity, unit price) |
| `MaGiamGias` | Discount codes (code, type, value, minimum order, usage limit, validity period, status) |
| `TinTucs` | News articles (title, summary, content, image, author, date) |
| `LienHes` | Contact messages (name, email, phone, subject, content, date, handled status) |

Primary keys, foreign keys, and constraints are used to enforce relationships between orders, order items, products, and accounts.

## Getting Started

### Prerequisites

- Windows with **Visual Studio** (a version that supports ASP.NET MVC 5 and .NET Framework)
- **SQL Server** (Express edition is sufficient)
- **SQL Server Management Studio** (optional, for managing the database)

### Installation

1. **Clone the repository**

   ```bash
   git clone <your-repository-url>
   cd <project-folder>
   ```

2. **Open the solution** in Visual Studio (`.sln` file).

3. **Restore NuGet packages** through *Tools → NuGet Package Manager → Restore* or by rebuilding the solution.

4. **Create the database**
   - Open SQL Server Management Studio and connect to your local SQL Server instance.
   - Run the database creation script included in the project (if provided), or let Entity Framework create the database from the models, depending on how the project is configured.
   - Load the sample data if a seed script is included.

5. **Configure the connection string** in `Web.config`. Replace the placeholder with your own server name:

   ```xml
   <connectionStrings>
     <add name="FruitBenContext"
          connectionString="Data Source=YOUR_SERVER_NAME;Initial Catalog=FruitBenDB;Integrated Security=True;"
          providerName="System.Data.SqlClient" />
   </connectionStrings>
   ```

   Replace `YOUR_SERVER_NAME` with your SQL Server instance name (for example, `localhost` or `.\SQLEXPRESS`). The connection name and database name must match the names used in the project.

6. **Run the application** by pressing `F5` or clicking *Start* in Visual Studio. The site opens in your default browser.

## Usage

### Customer flow

1. Browse the catalog or search for a fruit.
2. Open a product, choose a quantity, and click **Add to cart**.
3. Open the cart, review the total, and click **Checkout**.
4. Enter recipient details and the delivery address. Optionally apply a discount code.
5. Choose **COD** or **QR bank transfer** and place the order.
6. If you are logged in, track the order under **My Orders**. Pending orders can be cancelled.

### Administrator flow

1. Sign in with an admin account and open the dashboard.
2. Manage products, orders, customers, discount codes, and news from the side menu.
3. For bank transfer orders, verify the payment manually and update the order status.
4. Review sales statistics in the reporting section.

## Business Rules

- **Guest checkout**: Orders can be placed without an account. Guest orders are stored with recipient information and are not linked to a user account.
- **Account orders**: Orders placed by logged-in users are linked to their account and appear in their order history.
- **Cart storage**: The cart is kept in the user's session.
- **Order cancellation**: Customers can cancel only their own orders that are still *Pending*. The system checks ownership and status before applying the change.
- **Discount validation**: The system checks the code's validity period, usage limit, and minimum order value before calculating the discount.
- **Shipping fee**: Ho Chi Minh City orders incur a fixed fee, other provinces and cities incur a higher fixed fee, and orders above a subtotal threshold ship free.
- **Revenue statistics**: Cancelled orders are excluded from sales totals.
- **Bank transfer**: The site displays a QR code with transfer details. The administrator confirms the payment manually.

## Known Limitations

- Bank transfer payments are confirmed manually. There is no automatic reconciliation with banks.
- No integration with shipping carriers. Delivery tracking is managed manually.
- No support for multiple warehouses, branches, suppliers, or batch-level inventory.
- Expiry dates for fruit batches are not tracked.
- Concurrent-user performance has not been fully tested.
- The application is built on ASP.NET MVC 5 (.NET Framework), which runs only on Windows.

## Future Work

- Integrate online payment gateways with automatic transaction confirmation.
- Connect with shipping providers for live delivery tracking.
- Add product reviews, a wishlist, and order lookup for guest customers.
- Improve inventory management, expiry tracking, and sales reporting.
- Make the interface more responsive on mobile devices.
- Strengthen security and optimize performance under load.

## Team

This project was completed by a team of three students:

| Student ID | Name |
|---|---|
| 2001240169 | Hà Quốc Huy |
| 2001240003 | Nguyễn Bình An |
| 2001240441 | Giáp Chí Thanh |

**Supervisor:** ThS. Nguyễn Hải Yến

## License

This project was developed for academic purposes. Add a license here if you plan to publish or reuse the code.

## Acknowledgments

- Microsoft documentation for ASP.NET MVC 5, C#, Entity Framework, SQL Server, and ADO.NET
- MDN Web Docs for HTML, CSS, and JavaScript
- OWASP Authentication Cheat Sheet
- Reference websites for fruit product data and layout: Klever Fruit, Emartmall, and New Fresh Foods
