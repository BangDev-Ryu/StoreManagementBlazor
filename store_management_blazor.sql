CREATE TABLE `users` (
  `user_id` INT PRIMARY KEY AUTO_INCREMENT,
  `username` VARCHAR(50) UNIQUE NOT NULL,
  `password` VARCHAR(255) NOT NULL,
  `full_name` VARCHAR(100),
  `role` ENUM ('admin', 'staff', 'customer') DEFAULT 'staff',
  `created_at` TIMESTAMP DEFAULT (CURRENT_TIMESTAMP)
);

CREATE TABLE `customers` (
  `customer_id` INT PRIMARY KEY AUTO_INCREMENT,
  `user_id` INT,
  `name` VARCHAR(100) NOT NULL,
  `phone` VARCHAR(20),
  `email` VARCHAR(100),
  `address` TEXT,
  `created_at` TIMESTAMP DEFAULT (CURRENT_TIMESTAMP)
);

CREATE TABLE `categories` (
  `category_id` INT PRIMARY KEY AUTO_INCREMENT,
  `category_name` VARCHAR(100) NOT NULL
);

CREATE TABLE `suppliers` (
  `supplier_id` INT PRIMARY KEY AUTO_INCREMENT,
  `name` VARCHAR(100) NOT NULL,
  `phone` VARCHAR(20),
  `email` VARCHAR(100),
  `address` TEXT
);

CREATE TABLE `products` (
  `product_id` INT PRIMARY KEY AUTO_INCREMENT,
  `category_id` INT,
  `supplier_id` INT,
  `product_name` VARCHAR(100) NOT NULL,
  `barcode` VARCHAR(50) UNIQUE,
  `price` DECIMAL(10,2) NOT NULL,
  `unit` VARCHAR(20) DEFAULT 'pcs',
  `created_at` TIMESTAMP DEFAULT (CURRENT_TIMESTAMP)
);

CREATE TABLE `inventory` (
  `inventory_id` INT PRIMARY KEY AUTO_INCREMENT,
  `product_id` INT NOT NULL,
  `quantity` INT DEFAULT 0,
  `updated_at` TIMESTAMP DEFAULT (CURRENT_TIMESTAMP)
);

CREATE TABLE `promotions` (
  `promo_id` INT PRIMARY KEY AUTO_INCREMENT,
  `promo_code` VARCHAR(50) UNIQUE NOT NULL,
  `description` VARCHAR(255),
  `discount_type` ENUM ('percent', 'fixed') NOT NULL,
  `discount_value` DECIMAL(10,2) NOT NULL,
  `start_date` DATE NOT NULL,
  `end_date` DATE NOT NULL,
  `min_order_amount` DECIMAL(10,2) DEFAULT 0,
  `usage_limit` INT DEFAULT 0,
  `used_count` INT DEFAULT 0,
  `status` ENUM ('active', 'inactive') DEFAULT 'active'
);

CREATE TABLE `orders` (
  `order_id` INT PRIMARY KEY AUTO_INCREMENT,
  `customer_id` INT,
  `user_id` INT,
  `promo_id` INT,
  `order_date` TIMESTAMP DEFAULT (CURRENT_TIMESTAMP),
  `status` ENUM ('pending', 'paid', 'canceled') DEFAULT 'pending',
  `total_amount` DECIMAL(10,2),
  `discount_amount` DECIMAL(10,2) DEFAULT 0
);

CREATE TABLE `order_items` (
  `order_item_id` INT PRIMARY KEY AUTO_INCREMENT,
  `order_id` INT,
  `product_id` INT,
  `quantity` INT NOT NULL,
  `price` DECIMAL(10,2) NOT NULL,
  `subtotal` DECIMAL(10,2) NOT NULL
);

CREATE TABLE `payments` (
  `payment_id` INT PRIMARY KEY AUTO_INCREMENT,
  `order_id` INT NOT NULL,
  `amount` DECIMAL(10,2) NOT NULL,
  `payment_method` ENUM ('cash', 'card', 'bank_transfer', 'e-wallet') DEFAULT 'cash',
  `payment_date` TIMESTAMP DEFAULT (CURRENT_TIMESTAMP)
);

CREATE TABLE `carts` (
  `cart_id` INT PRIMARY KEY AUTO_INCREMENT,
  `customer_id` INT,
  `status` ENUM ('pending', 'done') DEFAULT 'pending',
  `total_amount` DECIMAL(10,2)
);

CREATE TABLE `cart_items` (
  `cart_item_id` INT PRIMARY KEY AUTO_INCREMENT,
  `cart_id` INT,
  `product_id` INT,
  `quantity` INT NOT NULL,
  `price` DECIMAL(10,2) NOT NULL,
  `subtotal` DECIMAL(10,2) NOT NULL
);
