<p align="center">
  <img src="public/images/logo.png" alt="PharmaPOS Logo" width="320"/>
</p>

<h1 align="center">PharmaPOS</h1>

<p align="center">
  <strong>A comprehensive Point-of-Sale & Pharmacy Management System</strong>
</p>

<p align="center">
  <a href="#features">Features</a> •
  <a href="#screenshots">Screenshots</a> •
  <a href="#tech-stack">Tech Stack</a> •
  <a href="#installation">Installation</a> •
  <a href="#database-setup">Database</a> •
  <a href="#project-structure">Structure</a> •
  <a href="#license">License</a>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/PHP-8.0%2B-777BB4?style=for-the-badge&logo=php&logoColor=white" alt="PHP"/>
  <img src="https://img.shields.io/badge/MySQL-8.0-4479A1?style=for-the-badge&logo=mysql&logoColor=white" alt="MySQL"/>
  <img src="https://img.shields.io/badge/Twig-3.x-bacf29?style=for-the-badge&logo=symfony&logoColor=white" alt="Twig"/>
  <img src="https://img.shields.io/badge/Bootstrap-4.x-7952B3?style=for-the-badge&logo=bootstrap&logoColor=white" alt="Bootstrap"/>
  <img src="https://img.shields.io/badge/jQuery-DataTables-0769AD?style=for-the-badge&logo=jquery&logoColor=white" alt="jQuery"/>
</p>

---

## 📋 Overview

**PharmaPOS** is a full-featured pharmacy point-of-sale and inventory management web application. It streamlines pharmacy operations — from medicine inventory tracking and purchase management to customer billing, stock adjustment, and financial reporting — all within a clean, responsive dashboard interface.

---

## ✨ Features

### 🏪 Point of Sale (POS) / Billing
- Create and manage customer bills with itemized medicine entries
- Auto-fetch medicine batches with sale price, tax, and discount calculations
- Support for multiple payment methods (Cash, Card, etc.)
- Bill view with printable invoice layout (PDF export via DomPDF)
- Date-range filtering and advanced search on billing records

### 💊 Medicine Management
- Add, edit, view, and delete medicines with detailed attributes (company, generic name, category, unit packing, store location, etc.)
- Batch-level tracking per medicine (batch number, expiry, quantity, sale/purchase prices)
- Medicine categories management (Tablet, Syrup, Liquid, etc.)
- Min-level and reorder-level alerts for low stock

### 📦 Purchase Management
- Record purchases from suppliers with per-batch breakdown
- Automatic stock replenishment on purchase entry
- Tax and discount applied at purchase level
- Purchase history with date-range filtering

### 📊 Stock Adjustment
- **Live Stock** — View all active inventory
- **Expired Stock** — Identify and manage expired medicines
- **Expiring Soon** — Filter medicines expiring within 1, 2, 3, or 6 months
- Manual stock quantity adjustments and batch deletion

### 👥 Customer Management
- Full customer database with name, email, phone, and gender
- Customer search with auto-complete for quick billing
- View customer-level billing history

### 📈 Reports & Analytics
- **POS/Bill Report** — Detailed billing reports with filters
- **Purchase Report** — Supplier purchase analysis
- **Inventory Report** — Stock valuation and movement
- **Income & Expense Stats** — Revenue, profit, and expense charts
- Admin dashboard with 12-month trend charts (revenue, income, transactions)

### 🔐 Authentication & User Management
- Secure login with hashed passwords (bcrypt)
- Forgot password & reset password via email (PHPMailer)
- Login attempt tracking for brute-force protection
- CSRF token validation on all POST requests

### 👤 Role-Based Access Control (RBAC)
- Granular permission system per user role
- Custom role creation with per-route permission assignment
- Pre-configured roles:

| Role | Description | Key Permissions |
|---|---|---|
| **Admin** | Full system access | All modules, user management, system settings, reports |
| **Store Manager** | Store operations | Billing, purchases, stock, medicines, customers, payment methods |
| **Employee** | Front-desk staff | Customer management, POS/billing (create, edit, view, delete) |

> Roles are fully customizable — create new roles and assign granular per-route permissions from the admin panel.

### ⚙️ System Settings
- Configurable pharmacy name, logo, favicon, and branding
- Contact information and address management
- Currency and date format configuration
- Media library with drag-and-drop image upload (Dropzone.js)
- Supplier management (name, email, phone, address)
- Payment method configuration

---

## 📸 Screenshots

### Login
![Login](public/resources/screenshots/login.png)

### POS / Billing
| New Bill | Billing List |
|---|---|
| ![New Bill](public/resources/screenshots/new_bill.png) | ![POS](public/resources/screenshots/pos.png) |

### Medicine Management
| Medicine List | Add Medicine | Medicine Categories |
|---|---|---|
| ![Medicines](public/resources/screenshots/medicines.png) | ![Add Medicine](public/resources/screenshots/add_medicine.png) | ![Categories](public/resources/screenshots/medicine_categories.png) |

### Purchase Management
| Purchase List | Add Purchase |
|---|---|
| ![Purchases](public/resources/screenshots/purchases.png) | ![Add Purchase](public/resources/screenshots/add_purchase.png) |

### Stock Adjustment
| Live Stock | Expired Stock |
|---|---|
| ![Live Stock](public/resources/screenshots/live_stock.png) | ![Expired Stock](public/resources/screenshots/expire_stock.png) |

### Customer Management
| Customer List | Add Customer |
|---|---|
| ![Customers](public/resources/screenshots/customers.png) | ![Add Customer](public/resources/screenshots/add_customer.png) |

### Reports
![Reports](public/resources/screenshots/reports.png)

### User Management & RBAC
| Users | Add User | User Roles | Permissions |
|---|---|---|---|
| ![Users](public/resources/screenshots/users.png) | ![Add User](public/resources/screenshots/add_user.png) | ![Roles](public/resources/screenshots/user_roles.png) | ![Permissions](public/resources/screenshots/user_permissions.png) |

### Settings & Configuration
| System Settings | Suppliers | Add Supplier | Payment Methods | Add Payment Method |
|---|---|---|---|---|
| ![Settings](public/resources/screenshots/settings.png) | ![Suppliers](public/resources/screenshots/suppliers.png) | ![Add Supplier](public/resources/screenshots/add_supplier.png) | ![Payment Methods](public/resources/screenshots/payment_methods.png) | ![Add Payment](public/resources/screenshots/add_payment_method.png) |

### Database Schema (ERD)
<p align="center">
  <img src="public/resources/screenshots/erd.png" alt="PharmaPOS ERD - Database Schema" width="100%"/>
</p>

---

## 🛠️ Tech Stack

### Backend
| Technology | Purpose |
|---|---|
| **PHP 8.0+** | Core server-side language |
| **Custom MVC Framework** | Lightweight framework with Router, Controller, Model, Loader, and Repository pattern |
| **Twig 3.x** | Template engine for view rendering |
| **MySQL 8.0** | Relational database with views and foreign key constraints |
| **PHPMailer 6.x** | Email delivery for password resets |
| **DomPDF 0.8.x** | PDF generation for invoices and reports |
| **HTML Purifier** | Input sanitization and XSS prevention |
| **Composer** | PHP dependency management |

### Frontend
| Technology | Purpose |
|---|---|
| **Bootstrap 4** | Responsive UI framework |
| **jQuery** | DOM manipulation and AJAX requests |
| **DataTables** | Advanced data tables with sorting, searching, and pagination |
| **Chart.js** | Dashboard analytics charts |
| **Flatpickr** | Modern date picker for forms |
| **Summernote** | Rich text editor for notes |
| **Dropzone.js** | Drag-and-drop file uploads |
| **Moment.js** | Date/time parsing and formatting |
| **Line Awesome** | Icon library |
| **Inter (Google Fonts)** | Typography |

---

## 🚀 Installation

### Prerequisites
- **PHP** >= 8.0
- **MySQL** >= 8.0
- **Composer** (for PHP dependencies)
- **Apache** with `mod_rewrite` enabled (or Nginx equivalent)
- A local development environment like [Laragon](https://laragon.org/), [XAMPP](https://www.apachefriends.org/), or [WAMP](https://www.wampserver.com/)

### Step 1: Clone the Repository
```bash
git clone https://github.com/nomanaadma/pharmaPOS.git
cd pharmaPOS
```

### Step 2: Install PHP Dependencies
```bash
cd builder
composer install
cd ..
```

### Step 3: Database Setup
1. Create a new MySQL database named `pharmaPOS`:
   ```sql
   CREATE DATABASE pharmaPOS;
   ```
2. Import the SQL dump:
   ```bash
   mysql -u root -p pharmaPOS < pharmapos.sql
   ```
   > Alternatively, use phpMyAdmin or any MySQL GUI to import `pharmapos.sql`. It is located inside `public/resources/` folder.

### Step 4: Configure the Application
Edit `config/config.php` and update the following:

```php
// Application URL (must match your local server setup)
define('URL', 'http://pharmapos.test/');

// Absolute path to the project root
define('DIR', 'D:/laragon/www/pharmaPOS/');

// Database credentials
define('DB_HOSTNAME', 'localhost');
define('DB_USERNAME', 'root');
define('DB_PASSWORD', '');
define('DB_DATABASE', 'pharmaPOS');
```

### Step 5: Launch
- If using **Laragon**, the app is accessible at `http://pharmapos.test/`
- If using **XAMPP/WAMP**, ensure the project is inside `htdocs` and visit `http://localhost/pharmaPOS/`

### Default Login Credentials
| Role | Username | Password |
|---|---|---|
| Admin | `nomanaadma` | `$upeEam#@!f0` |

---

## 🗄️ Database Setup

The database consists of the following tables:

| Table | Description |
|---|---|
| `users` | User accounts with roles and authentication |
| `user_role` | Role definitions with granular permissions |
| `customers` | Customer records |
| `medicines` | Medicine catalog |
| `medicine_category` | Medicine categories (Tablet, Syrup, etc.) |
| `medicine_batch` | Batch-level stock tracking (qty, expiry, prices) |
| `medicine_purchase` | Purchase orders from suppliers |
| `medicine_bill` | Sales/billing records |
| `bill_items` | Line items per bill |
| `suppliers` | Supplier contacts |
| `payment_method` | Payment method configuration |
| `menu` | Dynamic sidebar navigation |
| `setting` | System settings (JSON) |
| `media` | Uploaded media files |
| `login_attempts` | Brute-force protection tracking |

**Database Views:**
- `medicine_batch_view` — Enriched batch data with unit prices, total sales/purchase, and profit
- `bill_items_view` — Aggregated bill items by medicine and batch

---

## 📁 Project Structure

```
pharmaPOS/
├── index.php                  # Application entry point
├── config/
│   └── config.php             # Configuration (DB, URLs, keys)
├── builder/                   # Framework core
│   ├── startup.php            # Autoloader and initialization
│   ├── bootstrap.php          # Application bootstrapping
│   ├── routes.php             # Route definitions
│   ├── core/                  # MVC engine (Controller, Router, Model, Loader, Repository)
│   ├── library/               # Utility libraries (Database, Session, URL, Response, Template)
│   ├── storage/               # Storage directory
│   ├── vendor/                # Composer dependencies
│   └── composer.json          # PHP dependencies
├── app/
│   ├── http/
│   │   ├── controllers/       # Request handlers
│   │   │   ├── DashboardController.php
│   │   │   ├── MedicineController.php
│   │   │   ├── CustomerController.php
│   │   │   ├── LoginController.php
│   │   │   ├── UserController.php
│   │   │   ├── ReportController.php
│   │   │   ├── SettingController.php
│   │   │   ├── FinanceController.php
│   │   │   ├── ProfileController.php
│   │   │   ├── UploadController.php
│   │   │   └── ...
│   │   └── models/            # Database query layer
│   │       ├── Medicine.php
│   │       ├── Customer.php
│   │       ├── Dashboard.php
│   │       ├── Report.php
│   │       ├── User.php
│   │       └── ...
│   └── views/                 # Twig templates
│       ├── auth/              # Login, forgot/reset password
│       ├── dashboard/         # Admin & employee dashboards
│       ├── medicine/          # Medicine CRUD, billing, purchase, stock
│       ├── customer/          # Customer CRUD & view
│       ├── report/            # Reports (inventory, income/expense)
│       ├── user/              # User & role management
│       ├── setting/           # System info & suppliers
│       ├── finance/           # Payment methods
│       ├── profile/           # User profile
│       └── common/            # Shared header, footer, modals
├── public/
│   ├── css/                   # Stylesheets
│   ├── js/                    # JavaScript files
│   ├── images/                # Logo, favicon, backgrounds
│   ├── fonts/                 # Web fonts
│   ├── uploads/               # User-uploaded media
│   └── resources/             # Static resources like screenshots and database sql file
└── .gitignore
```

---



## 🔒 Security Features

- **Password Hashing** — bcrypt via PHP's `password_hash()`
- **CSRF Protection** — Token validation on all POST requests
- **XSS Prevention** — HTML Purifier for input sanitization
- **Login Attempt Tracking** — Brute-force protection with attempt counting
- **Session Management** — Secure session handling with user agent tracking
- **`.htaccess` Protection** — Directory access restriction on sensitive folders

---

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/your-feature`)
3. Commit your changes (`git commit -m 'Add your feature'`)
4. Push to the branch (`git push origin feature/your-feature`)
5. Open a Pull Request

---

## 📄 License

This project is open source and available under the [MIT License](LICENSE).

---

<p align="center">
  Made with ❤️ By Noman Shoukat
</p>
