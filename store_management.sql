
-- Bảng người dùng
CREATE TABLE users (
    user_id INT AUTO_INCREMENT PRIMARY KEY,
    username VARCHAR(50) UNIQUE NOT NULL,
    password VARCHAR(255) NOT NULL,
    full_name VARCHAR(100),
    role ENUM('admin','staff') DEFAULT 'staff',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Bảng khách hàng
CREATE TABLE customers (
    customer_id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    phone VARCHAR(20),
    email VARCHAR(100),
    address TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Bảng loại sản phẩm
CREATE TABLE categories (
    category_id INT AUTO_INCREMENT PRIMARY KEY,
    category_name VARCHAR(100) NOT NULL
);

-- Bảng nhà cung cấp
CREATE TABLE suppliers (
    supplier_id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    phone VARCHAR(20),
    email VARCHAR(100),
    address TEXT
);

-- Bảng sản phẩm
CREATE TABLE products (
    product_id INT AUTO_INCREMENT PRIMARY KEY,
    category_id INT,
    supplier_id INT,
    product_name VARCHAR(100) NOT NULL,
    barcode VARCHAR(50) UNIQUE,
    price DECIMAL(10,2) NOT NULL,
    unit VARCHAR(20) DEFAULT 'pcs',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Bảng tồn kho
CREATE TABLE inventory (
    inventory_id INT AUTO_INCREMENT PRIMARY KEY,
    product_id INT NOT NULL,
    quantity INT DEFAULT 0,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- Bảng khuyến mãi
CREATE TABLE promotions (
    promo_id INT AUTO_INCREMENT PRIMARY KEY,
    promo_code VARCHAR(50) UNIQUE NOT NULL,
    description VARCHAR(255),
    discount_type ENUM('percent','fixed') NOT NULL,
    discount_value DECIMAL(10,2) NOT NULL,
    start_date DATE NOT NULL,
    end_date DATE NOT NULL,
    min_order_amount DECIMAL(10,2) DEFAULT 0,
    usage_limit INT DEFAULT 0,
    used_count INT DEFAULT 0,
    status ENUM('active','inactive') DEFAULT 'active'
);

-- Bảng đơn hàng
CREATE TABLE orders (
    order_id INT AUTO_INCREMENT PRIMARY KEY,
    customer_id INT,
    user_id INT,
    promo_id INT NULL,
    order_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    status ENUM('pending','paid','canceled') DEFAULT 'pending',
    total_amount DECIMAL(10,2),
    discount_amount DECIMAL(10,2) DEFAULT 0
);

-- Bảng chi tiết đơn hàng
CREATE TABLE order_items (
    order_item_id INT AUTO_INCREMENT PRIMARY KEY,
    order_id INT,
    product_id INT,
    quantity INT NOT NULL,
    price DECIMAL(10,2) NOT NULL,
    subtotal DECIMAL(10,2) NOT NULL
);

-- Bảng thanh toán
CREATE TABLE payments (
    payment_id INT AUTO_INCREMENT PRIMARY KEY,
    order_id INT NOT NULL,
    amount DECIMAL(10,2) NOT NULL,
    payment_method ENUM('cash','card','bank_transfer','e-wallet') DEFAULT 'cash',
    payment_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- DATA USERS
INSERT INTO users (username,password,full_name,role) VALUES
('admin','123456','Quản trị viên','admin'),
('staff01','123456','Nguyễn Văn A','staff'),
('staff02','123456','Lê Thị B','staff');

-- DATA CUSTOMERS
INSERT INTO customers (name,phone,email,address) VALUES
('Khách hàng 1', '0909000001', 'kh1@mail.com', 'Địa chỉ 1'),('Khách hàng 2', '0909000002', 'kh2@mail.com', 'Địa chỉ 2'),('Khách hàng 3', '0909000003', 'kh3@mail.com', 'Địa chỉ 3'),('Khách hàng 4', '0909000004', 'kh4@mail.com', 'Địa chỉ 4'),('Khách hàng 5', '0909000005', 'kh5@mail.com', 'Địa chỉ 5'),('Khách hàng 6', '0909000006', 'kh6@mail.com', 'Địa chỉ 6'),('Khách hàng 7', '0909000007', 'kh7@mail.com', 'Địa chỉ 7'),('Khách hàng 8', '0909000008', 'kh8@mail.com', 'Địa chỉ 8'),('Khách hàng 9', '0909000009', 'kh9@mail.com', 'Địa chỉ 9'),('Khách hàng 10', '0909000010', 'kh10@mail.com', 'Địa chỉ 10'),('Khách hàng 11', '0909000011', 'kh11@mail.com', 'Địa chỉ 11'),('Khách hàng 12', '0909000012', 'kh12@mail.com', 'Địa chỉ 12'),('Khách hàng 13', '0909000013', 'kh13@mail.com', 'Địa chỉ 13'),('Khách hàng 14', '0909000014', 'kh14@mail.com', 'Địa chỉ 14'),('Khách hàng 15', '0909000015', 'kh15@mail.com', 'Địa chỉ 15'),('Khách hàng 16', '0909000016', 'kh16@mail.com', 'Địa chỉ 16'),('Khách hàng 17', '0909000017', 'kh17@mail.com', 'Địa chỉ 17'),('Khách hàng 18', '0909000018', 'kh18@mail.com', 'Địa chỉ 18'),('Khách hàng 19', '0909000019', 'kh19@mail.com', 'Địa chỉ 19'),('Khách hàng 20', '0909000020', 'kh20@mail.com', 'Địa chỉ 20');

-- DATA CATEGORIES
INSERT INTO categories (category_name) VALUES
('Đồ uống'),('Bánh kẹo'),('Gia vị'),('Đồ gia dụng'),('Mỹ phẩm');

-- DATA SUPPLIERS
INSERT INTO suppliers (name,phone,email,address) VALUES
('Công ty ABC','0909123456','abc@gmail.com','Hà Nội'),
('Công ty XYZ','0912123456','xyz@gmail.com','TP HCM'),
('Công ty 123','0933123456','123@gmail.com','Đà Nẵng');

-- DATA PRODUCTS
INSERT INTO products (category_id,supplier_id,product_name,barcode,price,unit) VALUES
(2, 1, 'Coca Cola lon', '8900000000001', 314838, 'hộp'),(1, 3, 'Pepsi lon', '8900000000002', 114807, 'cái'),(3, 3, 'Trà Xanh 0 độ', '8900000000003', 415725, 'tuýp'),(2, 1, 'Sting dâu', '8900000000004', 351670, 'cái'),(3, 2, 'Red Bull', '8900000000005', 402179, 'lon'),(2, 2, 'Bánh Oreo', '8900000000006', 209283, 'chai'),(5, 3, 'Bánh Chocopie', '8900000000007', 212528, 'lon'),(1, 2, 'Kẹo Alpenliebe', '8900000000008', 34313, 'lon'),(5, 1, 'Kẹo bạc hà', '8900000000009', 316289, 'cái'),(1, 2, 'Socola KitKat', '8900000000010', 139959, 'chai'),(5, 1, 'Nước mắm Nam Ngư', '8900000000011', 51792, 'chai'),(2, 2, 'Nước tương Maggi', '8900000000012', 462539, 'lon'),(5, 3, 'Muối i-ốt', '8900000000013', 173302, 'cái'),(1, 1, 'Bột ngọt Ajinomoto', '8900000000014', 443069, 'cái'),(2, 2, 'Dầu ăn Tường An', '8900000000015', 281354, 'tuýp'),(2, 1, 'Nồi cơm điện', '8900000000016', 405347, 'hộp'),(1, 3, 'Ấm siêu tốc', '8900000000017', 113087, 'chai'),(3, 2, 'Quạt máy', '8900000000018', 69968, 'hộp'),(4, 1, 'Bếp gas mini', '8900000000019', 416845, 'lon'),(3, 3, 'Máy xay sinh tố', '8900000000020', 334564, 'hộp'),(1, 1, 'Sữa rửa mặt Hazeline', '8900000000021', 188475, 'lon'),(4, 1, 'Kem dưỡng da Ponds', '8900000000022', 413840, 'hộp'),(3, 3, 'Dầu gội Sunsilk', '8900000000023', 158950, 'tuýp'),(4, 2, 'Sữa tắm Dove', '8900000000024', 336928, 'chai'),(1, 1, 'Nước hoa Romano', '8900000000025', 352508, 'cái'),(1, 1, 'Cà phê G7', '8900000000026', 201228, 'lon'),(2, 1, 'Trà Lipton', '8900000000027', 38039, 'cái'),(2, 3, 'Sữa Vinamilk', '8900000000028', 252845, 'chai'),(3, 1, 'Sữa TH True Milk', '8900000000029', 35278, 'hộp'),(3, 2, 'Nước suối Lavie', '8900000000030', 331637, 'lon'),(5, 3, 'Khăn giấy Tempo', '8900000000031', 102525, 'chai'),(4, 3, 'Giấy vệ sinh Pulppy', '8900000000032', 495429, 'chai'),(3, 2, 'Bình nước Lock&Lock', '8900000000033', 354771, 'gói'),(2, 1, 'Hộp nhựa Tupperware', '8900000000034', 297415, 'cái'),(1, 3, 'Dao Inox', '8900000000035', 47523, 'hộp'),(3, 1, 'Bàn chải Colgate', '8900000000036', 136417, 'chai'),(2, 2, 'Kem đánh răng P/S', '8900000000037', 93713, 'hộp'),(2, 3, 'Nước súc miệng Listerine', '8900000000038', 223906, 'gói'),(1, 2, 'Bông tẩy trang', '8900000000039', 317819, 'tuýp'),(4, 1, 'Khẩu trang 3M', '8900000000040', 464252, 'gói'),(3, 1, 'Bánh mì sandwich', '8900000000041', 279350, 'cái'),(5, 2, 'Mì gói Hảo Hảo', '8900000000042', 9413, 'hộp'),(1, 2, 'Mì Omachi', '8900000000043', 26616, 'hộp'),(5, 2, 'Bún khô', '8900000000044', 350911, 'gói'),(3, 1, 'Phở ăn liền', '8900000000045', 407779, 'tuýp'),(1, 1, 'Nước ngọt Sprite', '8900000000046', 230083, 'hộp'),(1, 3, 'Trà sữa đóng chai', '8900000000047', 15130, 'cái'),(3, 3, 'Snack Oishi', '8900000000048', 43415, 'cái'),(4, 2, 'Snack Lays', '8900000000049', 83536, 'tuýp'),(1, 2, 'Kẹo dẻo Haribo', '8900000000050', 328680, 'cái');

-- DATA INVENTORY
INSERT INTO inventory (product_id,quantity) VALUES
(1, 25),(2, 169),(3, 77),(4, 169),(5, 90),(6, 105),(7, 125),(8, 37),(9, 74),(10, 149),(11, 69),(12, 23),(13, 46),(14, 144),(15, 134),(16, 182),(17, 99),(18, 72),(19, 128),(20, 123),(21, 155),(22, 78),(23, 166),(24, 117),(25, 168),(26, 197),(27, 36),(28, 145),(29, 61),(30, 139),(31, 47),(32, 154),(33, 194),(34, 41),(35, 154),(36, 71),(37, 49),(38, 165),(39, 73),(40, 176),(41, 41),(42, 34),(43, 175),(44, 59),(45, 198),(46, 106),(47, 99),(48, 55),(49, 62),(50, 33);

-- DATA PROMOTIONS
INSERT INTO promotions (promo_code,description,discount_type,discount_value,start_date,end_date,min_order_amount,usage_limit,status) VALUES
('SALE10', 'Giảm 10% cho mọi đơn hàng', 'percent', 10, '2025-01-01', '2025-12-31', 0, 0, 'active'),('FREESHIP50K', 'Giảm 50,000 cho đơn từ 300,000 trở lên', 'fixed', 50000, '2025-03-01', '2025-12-31', 300000, 500, 'active'),('NEWUSER', 'Giảm 20% cho khách hàng mới', 'percent', 20, '2025-01-01', '2025-06-30', 0, 1, 'active'),('SUMMER15', 'Giảm 15% mùa hè', 'percent', 15, '2025-06-01', '2025-08-31', 50000, 1000, 'active'),('VIP100K', 'Giảm 100,000 cho đơn từ 1 triệu', 'fixed', 100000, '2025-01-01', '2025-12-31', 1000000, 200, 'active');

-- DATA ORDERS
INSERT INTO orders (customer_id,user_id,promo_id,status,total_amount,discount_amount) VALUES
(5, 3, 5, 'paid', 1292330, 100000),(17, 3, NULL, 'paid', 1731608, 0),(8, 3, NULL, 'paid', 720782, 0),(20, 3, 5, 'paid', 21686, 21686),(1, 2, NULL, 'paid', 94180, 0),(5, 3, 2, 'paid', 3888671, 100000),(9, 3, 4, 'paid', 512594, 102518.8),(11, 3, 3, 'paid', 1715029, 171502.90000000002),(11, 3, NULL, 'paid', 2484051, 0),(11, 3, 2, 'paid', 1070239, 100000),(20, 3, NULL, 'paid', 1532741, 0),(10, 2, NULL, 'paid', 1785354, 0),(10, 3, 2, 'paid', 1588276, 100000),(6, 2, 2, 'paid', 2896096, 50000),(10, 2, 3, 'paid', 186000, 27900.0),(10, 2, 5, 'paid', 1024090, 50000),(19, 3, NULL, 'paid', 467148, 0),(10, 2, NULL, 'paid', 394342, 0),(8, 3, 4, 'paid', 1965637, 294845.55),(3, 3, NULL, 'paid', 2889813, 0),(9, 2, NULL, 'paid', 2288406, 0),(17, 3, NULL, 'paid', 331008, 0),(6, 3, 1, 'paid', 2154851, 323227.64999999997),(1, 3, 1, 'paid', 1138686, 170802.9),(2, 2, 5, 'paid', 393847, 100000),(15, 3, 1, 'paid', 260658, 52131.600000000006),(4, 2, NULL, 'paid', 933199, 0),(16, 2, NULL, 'paid', 2609123, 0),(4, 3, 4, 'paid', 2406292, 481258.4),(1, 3, NULL, 'paid', 2912134, 0);

-- DATA ORDER_ITEMS
INSERT INTO order_items (order_id,product_id,quantity,price,subtotal) VALUES
(1, 23, 2, 31265, 62530),(1, 5, 2, 205683, 411366),(1, 47, 1, 477948, 477948),(1, 25, 2, 170243, 340486),(2, 39, 1, 447059, 447059),(2, 14, 1, 51108, 51108),(2, 46, 3, 411147, 1233441),(3, 18, 3, 202167, 606501),(3, 34, 1, 44219, 44219),(3, 26, 3, 23354, 70062),(4, 24, 2, 10843, 21686),(5, 9, 1, 94180, 94180),(6, 18, 3, 186886, 560658),(6, 22, 2, 199267, 398534),(6, 42, 3, 215726, 647178),(6, 17, 3, 474268, 1422804),(6, 20, 3, 286499, 859497),(7, 8, 2, 256297, 512594),(8, 42, 1, 355116, 355116),(8, 43, 2, 129224, 258448),(8, 31, 3, 367155, 1101465),(9, 17, 2, 48755, 97510),(9, 12, 2, 381904, 763808),(9, 43, 2, 167445, 334890),(9, 19, 3, 429281, 1287843),(10, 25, 1, 232635, 232635),(10, 1, 2, 245362, 490724),(10, 23, 2, 127233, 254466),(10, 49, 2, 46207, 92414),(11, 3, 2, 347879, 695758),(11, 23, 3, 130215, 390645),(11, 4, 1, 64761, 64761),(11, 33, 1, 240159, 240159),(11, 7, 1, 141418, 141418),(12, 40, 2, 455428, 910856),(12, 46, 2, 75412, 150824),(12, 34, 2, 189856, 379712),(12, 25, 3, 114654, 343962),(13, 24, 2, 143251, 286502),(13, 23, 2, 381347, 762694),(13, 18, 2, 179146, 358292),(13, 9, 2, 90394, 180788),(14, 24, 2, 327016, 654032),(14, 2, 1, 403478, 403478),(14, 27, 3, 404474, 1213422),(14, 4, 2, 312582, 625164),(15, 18, 1, 105328, 105328),(15, 27, 2, 17303, 34606),(15, 50, 2, 23033, 46066),(16, 15, 1, 43160, 43160),(16, 16, 2, 18541, 37082),(16, 44, 1, 492698, 492698),(16, 41, 1, 451150, 451150),(17, 42, 1, 467148, 467148),(18, 30, 1, 64334, 64334),(18, 11, 1, 178454, 178454),(18, 20, 3, 50518, 151554),(19, 16, 1, 89280, 89280),(19, 23, 3, 404655, 1213965),(19, 11, 2, 331196, 662392),(20, 49, 1, 367325, 367325),(20, 32, 2, 264392, 528784),(20, 19, 3, 345903, 1037709),(20, 17, 2, 392028, 784056),(20, 19, 1, 171939, 171939),(21, 11, 3, 227666, 682998),(21, 25, 2, 436122, 872244),(21, 48, 1, 340400, 340400),(21, 10, 2, 58482, 116964),(21, 4, 2, 137900, 275800),(22, 40, 2, 165504, 331008),(23, 1, 2, 296698, 593396),(23, 16, 3, 384657, 1153971),(23, 40, 3, 135828, 407484),(24, 3, 3, 379562, 1138686),(25, 9, 1, 22063, 22063),(25, 16, 2, 185892, 371784),(26, 47, 2, 130329, 260658),(27, 37, 1, 448581, 448581),(27, 23, 1, 484618, 484618),(28, 20, 3, 357837, 1073511),(28, 34, 1, 161219, 161219),(28, 1, 3, 458131, 1374393),(29, 28, 1, 485514, 485514),(29, 7, 3, 487044, 1461132),(29, 42, 1, 235885, 235885),(29, 38, 1, 223761, 223761),(30, 25, 1, 426943, 426943),(30, 11, 3, 130209, 390627),(30, 5, 2, 73116, 146232),(30, 46, 2, 272220, 544440),(30, 23, 3, 467964, 1403892);

-- DATA PAYMENTS
INSERT INTO payments (order_id,amount,payment_method) VALUES
(1, 1192330, 'cash'),(2, 1731608, 'e-wallet'),(3, 720782, 'e-wallet'),(4, 0, 'card'),(5, 94180, 'cash'),(6, 3788671, 'cash'),(7, 410075.2, 'e-wallet'),(8, 1543526.1, 'cash'),(9, 2484051, 'cash'),(10, 970239, 'card'),(11, 1532741, 'e-wallet'),(12, 1785354, 'card'),(13, 1488276, 'card'),(14, 2846096, 'cash'),(15, 158100.0, 'card'),(16, 974090, 'cash'),(17, 467148, 'cash'),(18, 394342, 'e-wallet'),(19, 1670791.45, 'card'),(20, 2889813, 'card'),(21, 2288406, 'cash'),(22, 331008, 'e-wallet'),(23, 1831623.35, 'cash'),(24, 967883.1, 'e-wallet'),(25, 293847, 'cash'),(26, 208526.4, 'cash'),(27, 933199, 'cash'),(28, 2609123, 'card'),(29, 1925033.6, 'cash'),(30, 2912134, 'card');