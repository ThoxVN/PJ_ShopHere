/* =========================================================
   1) TẠO DATABASE & USE
   ========================================================= */
IF DB_ID(N'shophere') IS NOT NULL
BEGIN
  ALTER DATABASE shophere SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
  DROP DATABASE shophere;
END;
GO

CREATE DATABASE shophere;
GO
USE shophere;
GO


/* =========================================================
   2) BẢNG LÕI
   ========================================================= */

-- USERS
IF OBJECT_ID('dbo.users','U') IS NOT NULL DROP TABLE dbo.users;
CREATE TABLE dbo.users (
  userid           INT IDENTITY(1,1) PRIMARY KEY,
  email            NVARCHAR(255) NOT NULL UNIQUE,
  password_hash    NVARCHAR(255) NOT NULL,
  full_name        NVARCHAR(255) NULL,
  phone            NVARCHAR(50)  NULL,
  role             VARCHAR(20)   NOT NULL DEFAULT 'customer', -- 'customer' | 'admin'
  status           VARCHAR(20)   NOT NULL DEFAULT 'active',   -- 'active' | 'locked'
  created_at       DATETIME2(0)  NOT NULL DEFAULT SYSUTCDATETIME(),
  updated_at       DATETIME2(0)  NOT NULL DEFAULT SYSUTCDATETIME(),
  CONSTRAINT CK_users_role   CHECK (role   IN ('customer','admin')),
  CONSTRAINT CK_users_status CHECK (status IN ('active','locked'))
);
GO

-- ADDRESSES
IF OBJECT_ID('dbo.addresses','U') IS NOT NULL DROP TABLE dbo.addresses;
CREATE TABLE dbo.addresses (
  id               INT IDENTITY(1,1) PRIMARY KEY,
  user_id          INT NULL,
  type             VARCHAR(20)  NOT NULL, -- 'shipping' | 'billing'
  recipient_name   NVARCHAR(255) NULL,
  phone            NVARCHAR(50)  NULL,
  street           NVARCHAR(255) NULL,
  ward             NVARCHAR(255) NULL,
  district         NVARCHAR(255) NULL,
  city             NVARCHAR(255) NULL,
  postal_code      NVARCHAR(50)  NULL,
  country          NVARCHAR(50)  NOT NULL DEFAULT N'VN',
  is_default       BIT           NOT NULL DEFAULT 0,
  created_at       DATETIME2(0)  NOT NULL DEFAULT SYSUTCDATETIME(),
  updated_at       DATETIME2(0)  NOT NULL DEFAULT SYSUTCDATETIME(),
  CONSTRAINT FK_addresses_users
    FOREIGN KEY (user_id) REFERENCES dbo.users(userid) ON DELETE CASCADE,
  CONSTRAINT CK_addresses_type CHECK (type IN ('shipping','billing'))
);
GO

-- CATEGORIES
IF OBJECT_ID('dbo.categories','U') IS NOT NULL DROP TABLE dbo.categories;
CREATE TABLE dbo.categories (
  id               INT IDENTITY(1,1) PRIMARY KEY,
  name             NVARCHAR(255) NOT NULL,
  slug             NVARCHAR(255) NOT NULL UNIQUE,
  parent_id        INT NULL,
  is_active        BIT NOT NULL DEFAULT 1,
  created_at       DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
  updated_at       DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
  CONSTRAINT FK_categories_parent
    FOREIGN KEY (parent_id) REFERENCES dbo.categories(id) ON DELETE NO ACTION
);
GO
CREATE INDEX IX_categories_name ON dbo.categories(name);
GO

-- PRODUCTS
IF OBJECT_ID('dbo.products','U') IS NOT NULL DROP TABLE dbo.products;
CREATE TABLE dbo.products (
  id               INT IDENTITY(1,1) PRIMARY KEY,
  category_id      INT NULL,
  sku              VARCHAR(255)   NOT NULL UNIQUE,
  name             NVARCHAR(255)  NOT NULL,
  description_html NVARCHAR(MAX)  NULL,
  material         NVARCHAR(255)  NULL,
  is_active        BIT            NOT NULL DEFAULT 1,
  created_at       DATETIME2(0)   NOT NULL DEFAULT SYSUTCDATETIME(),
  updated_at       DATETIME2(0)   NOT NULL DEFAULT SYSUTCDATETIME(),
  CONSTRAINT FK_products_categories
    FOREIGN KEY (category_id) REFERENCES dbo.categories(id) ON DELETE SET NULL
);
GO
CREATE INDEX IX_products_name ON dbo.products(name);
GO


/* =========================================================
   3) BIẾN THỂ & TỒN KHO (giữ như bạn đang tham chiếu)
   ========================================================= */

-- PRODUCT_VARIANTS
IF OBJECT_ID('dbo.product_variants','U') IS NOT NULL DROP TABLE dbo.product_variants;
CREATE TABLE dbo.product_variants (
  id               INT IDENTITY(1,1) PRIMARY KEY,
  product_id       INT NOT NULL,
  sku              VARCHAR(255)  NOT NULL UNIQUE,
  name_extension   NVARCHAR(255) NULL,
  size             VARCHAR(20)   NOT NULL,   -- ví dụ: XS,S,M,L,XL,XXL
  color            NVARCHAR(50)  NOT NULL,   -- ví dụ: Black/White/Red...
  price_modifier   DECIMAL(12,2) NOT NULL DEFAULT (0),
  is_active        BIT           NOT NULL DEFAULT 1,
  created_at       DATETIME2(0)  NOT NULL DEFAULT SYSUTCDATETIME(),
  updated_at       DATETIME2(0)  NOT NULL DEFAULT SYSUTCDATETIME(),
  CONSTRAINT FK_variants_product
    FOREIGN KEY (product_id) REFERENCES dbo.products(id) ON DELETE CASCADE,
  CONSTRAINT UQ_variants_product_size_color UNIQUE (product_id, size, color)
);
GO
CREATE INDEX IX_variants_product ON dbo.product_variants(product_id);
GO

-- INVENTORIES
IF OBJECT_ID('dbo.inventories','U') IS NOT NULL DROP TABLE dbo.inventories;
CREATE TABLE dbo.inventories (
  id               INT IDENTITY(1,1) PRIMARY KEY,
  variant_id       INT NOT NULL UNIQUE,
  qty_on_hand      INT NOT NULL DEFAULT (0),
  qty_reserved     INT NOT NULL DEFAULT (0),
  updated_at       DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
  CONSTRAINT FK_inventories_variant
    FOREIGN KEY (variant_id) REFERENCES dbo.product_variants(id) ON DELETE CASCADE,
  CONSTRAINT CK_inventories_qty_on_hand  CHECK (qty_on_hand  >= 0),
  CONSTRAINT CK_inventories_qty_reserved CHECK (qty_reserved >= 0)
);
GO


/* =========================================================
   4) (Optional) REVIEWS (bạn có thể bỏ nếu chưa cần)
   ========================================================= */

IF OBJECT_ID('dbo.product_reviews','U') IS NOT NULL DROP TABLE dbo.product_reviews;
CREATE TABLE dbo.product_reviews (
  id               INT IDENTITY(1,1) PRIMARY KEY,
  product_id       INT NOT NULL,
  user_id          INT NULL,
  rating           TINYINT NOT NULL,
  title            NVARCHAR(255) NULL,
  content          NVARCHAR(MAX) NULL,
  is_approved      BIT NOT NULL DEFAULT 0,
  created_at       DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
  updated_at       DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
  CONSTRAINT FK_reviews_product
    FOREIGN KEY (product_id) REFERENCES dbo.products(id) ON DELETE CASCADE,
  CONSTRAINT FK_reviews_user
    FOREIGN KEY (user_id) REFERENCES dbo.users(userid) ON DELETE SET NULL,
  CONSTRAINT CK_reviews_rating CHECK (rating BETWEEN 1 AND 5),
  CONSTRAINT UQ_reviews_product_user UNIQUE (product_id, user_id)
);
GO
CREATE INDEX IX_reviews_product ON dbo.product_reviews(product_id);
CREATE INDEX IX_reviews_user    ON dbo.product_reviews(user_id);
GO


/* =========================================================
   5) CART & CART_ITEMS
   ========================================================= */

IF OBJECT_ID('dbo.carts','U') IS NOT NULL DROP TABLE dbo.carts;
CREATE TABLE dbo.carts (
  id               INT IDENTITY(1,1) PRIMARY KEY,
  user_id          INT NULL,
  session_id       VARCHAR(255) NULL UNIQUE, -- guest cart
  currency         VARCHAR(10)  NOT NULL DEFAULT 'VND',
  status           VARCHAR(20)  NOT NULL DEFAULT 'active',  -- active|converted|abandoned
  subtotal_amount  DECIMAL(12,2) NOT NULL DEFAULT (0),
  discount_amount  DECIMAL(12,2) NOT NULL DEFAULT (0),
  shipping_fee     DECIMAL(12,2) NOT NULL DEFAULT (0),
  tax_amount       DECIMAL(12,2) NOT NULL DEFAULT (0),
  grand_total      DECIMAL(12,2) NOT NULL DEFAULT (0),
  created_at       DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
  updated_at       DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
  CONSTRAINT FK_carts_user
    FOREIGN KEY (user_id) REFERENCES dbo.users(userid) ON DELETE SET NULL,
  CONSTRAINT CK_carts_status CHECK (status IN ('active','converted','abandoned')),
  CONSTRAINT CK_carts_subtotal CHECK (subtotal_amount >= 0),
  CONSTRAINT CK_carts_discount CHECK (discount_amount >= 0),
  CONSTRAINT CK_carts_shipfee CHECK (shipping_fee    >= 0),
  CONSTRAINT CK_carts_tax     CHECK (tax_amount      >= 0),
  CONSTRAINT CK_carts_grand   CHECK (grand_total     >= 0)
);
GO
CREATE INDEX IX_carts_user   ON dbo.carts(user_id);
CREATE INDEX IX_carts_status ON dbo.carts(status);
GO

IF OBJECT_ID('dbo.cart_items','U') IS NOT NULL DROP TABLE dbo.cart_items;
CREATE TABLE dbo.cart_items (
  id                   INT IDENTITY(1,1) PRIMARY KEY,
  cart_id              INT NOT NULL,
  variant_id           INT NOT NULL,
  quantity             INT NOT NULL,
  unit_price           DECIMAL(12,2) NOT NULL,
  line_discount_amount DECIMAL(12,2) NOT NULL DEFAULT (0),
  line_total           DECIMAL(12,2) NOT NULL,
  created_at           DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
  updated_at           DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
  CONSTRAINT FK_cart_items_cart
    FOREIGN KEY (cart_id)    REFERENCES dbo.carts(id)              ON DELETE CASCADE,
  CONSTRAINT FK_cart_items_variant
    FOREIGN KEY (variant_id) REFERENCES dbo.product_variants(id)   ON DELETE NO ACTION,
  CONSTRAINT CK_cart_items_qty     CHECK (quantity > 0),
  CONSTRAINT CK_cart_items_price   CHECK (unit_price >= 0),
  CONSTRAINT CK_cart_items_disc    CHECK (line_discount_amount >= 0),
  CONSTRAINT CK_cart_items_total   CHECK (line_total >= 0),
  CONSTRAINT UQ_cart_item_one_variant UNIQUE (cart_id, variant_id)
);
GO
CREATE INDEX IX_cart_items_cart ON dbo.cart_items(cart_id);
GO


/* =========================================================
   6) ORDERS & ORDER_ITEMS
   ========================================================= */
IF OBJECT_ID('dbo.orders','U') IS NOT NULL DROP TABLE dbo.orders;
CREATE TABLE dbo.orders (
  id                  INT IDENTITY(1,1) PRIMARY KEY,
  order_number        VARCHAR(50)  NOT NULL UNIQUE,
  user_id             INT NULL,
  cart_id             INT NULL,
  status              VARCHAR(30)  NOT NULL DEFAULT 'pending_payment',
  payment_status      VARCHAR(30)  NOT NULL DEFAULT 'unpaid',
  payment_method      VARCHAR(30)  NULL,    -- cod|card|bank_transfer|e_wallet
  paid_at             DATETIME2(0) NULL,
  shipping_method     VARCHAR(30)  NULL,    -- standard|express
  tracking_number     NVARCHAR(100) NULL,
  shipped_at          DATETIME2(0) NULL,
  delivered_at        DATETIME2(0) NULL,
  shipping_address_id INT NULL,
  billing_address_id  INT NULL,
  subtotal_amount     DECIMAL(12,2) NOT NULL DEFAULT (0),
  discount_amount     DECIMAL(12,2) NOT NULL DEFAULT (0),
  shipping_fee        DECIMAL(12,2) NOT NULL DEFAULT (0),
  tax_amount          DECIMAL(12,2) NOT NULL DEFAULT (0),
  grand_total         DECIMAL(12,2) NOT NULL DEFAULT (0),
  currency            VARCHAR(10)   NOT NULL DEFAULT 'VND',
  customer_note       NVARCHAR(MAX) NULL,
  created_at          DATETIME2(0)  NOT NULL DEFAULT SYSUTCDATETIME(),
  updated_at          DATETIME2(0)  NOT NULL DEFAULT SYSUTCDATETIME(),

CONSTRAINT FK_orders_user
    FOREIGN KEY (user_id)             REFERENCES dbo.users(userid)    ON DELETE SET NULL,
  CONSTRAINT FK_orders_cart
    FOREIGN KEY (cart_id)             REFERENCES dbo.carts(id)        ON DELETE SET NULL,
  CONSTRAINT FK_orders_ship_address
    FOREIGN KEY (shipping_address_id) REFERENCES dbo.addresses(id)    ON DELETE NO ACTION,
  CONSTRAINT FK_orders_bill_address
    FOREIGN KEY (billing_address_id)  REFERENCES dbo.addresses(id)    ON DELETE NO ACTION,

  CONSTRAINT CK_orders_status    CHECK (status IN ('pending_payment','paid','processing','shipped','completed','cancelled','refunded')),
  CONSTRAINT CK_orders_paystatus CHECK (payment_status IN ('unpaid','paid','refunded','partial_refund')),
  CONSTRAINT CK_orders_subtotal  CHECK (subtotal_amount >= 0),
  CONSTRAINT CK_orders_discount  CHECK (discount_amount >= 0),
  CONSTRAINT CK_orders_shipfee   CHECK (shipping_fee    >= 0),
  CONSTRAINT CK_orders_tax       CHECK (tax_amount      >= 0),
  CONSTRAINT CK_orders_grand     CHECK (grand_total     >= 0)
);
GO
CREATE INDEX IX_orders_user   ON dbo.orders(user_id);
CREATE INDEX IX_orders_status ON dbo.orders(status);
GO

IF OBJECT_ID('dbo.order_items','U') IS NOT NULL DROP TABLE dbo.order_items;
CREATE TABLE dbo.order_items (
  id                      INT IDENTITY(1,1) PRIMARY KEY,
  order_id                INT NOT NULL,
  variant_id              INT NOT NULL,
  product_name_snapshot   NVARCHAR(255) NOT NULL,
  sku_snapshot            VARCHAR(255)  NOT NULL,
  variant_name_snapshot   NVARCHAR(255) NULL,
  quantity                INT NOT NULL,
  unit_price              DECIMAL(12,2) NOT NULL,
  line_discount_amount    DECIMAL(12,2) NOT NULL DEFAULT (0),
  line_total              DECIMAL(12,2) NOT NULL,
  created_at              DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
  updated_at              DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
  CONSTRAINT FK_order_items_order
    FOREIGN KEY (order_id)  REFERENCES dbo.orders(id)            ON DELETE CASCADE,
  CONSTRAINT FK_order_items_variant
    FOREIGN KEY (variant_id) REFERENCES dbo.product_variants(id) ON DELETE NO ACTION,
  CONSTRAINT CK_order_items_qty   CHECK (quantity > 0),
  CONSTRAINT CK_order_items_price CHECK (unit_price >= 0),
  CONSTRAINT CK_order_items_disc  CHECK (line_discount_amount >= 0),
  CONSTRAINT CK_order_items_total CHECK (line_total >= 0)
);
GO
CREATE INDEX IX_order_items_order ON dbo.order_items(order_id);
GO



/* =========================================================
   7) TRIGGERS updated_at (mỗi bảng 1 trigger)
   ========================================================= */
-- Pattern: cập nhật updated_at khi UPDATE (tránh vòng lặp vô hạn)
-- Lưu ý: SQL Server không cho 1 trigger dùng chung nhiều bảng, nên tạo riêng.

CREATE OR ALTER TRIGGER trg_users_updated_at
ON dbo.users
AFTER UPDATE
AS
BEGIN
  SET NOCOUNT ON;
  UPDATE u SET updated_at = SYSUTCDATETIME()
  FROM dbo.users u
  INNER JOIN inserted i ON i.userid = u.userid;
END;
GO

CREATE OR ALTER TRIGGER trg_addresses_updated_at
ON dbo.addresses
AFTER UPDATE
AS
BEGIN
  SET NOCOUNT ON;
  UPDATE a SET updated_at = SYSUTCDATETIME()
  FROM dbo.addresses a
  INNER JOIN inserted i ON i.id = a.id;
END;
GO

CREATE OR ALTER TRIGGER trg_categories_updated_at
ON dbo.categories
AFTER UPDATE
AS
BEGIN
  SET NOCOUNT ON;
  UPDATE c SET updated_at = SYSUTCDATETIME()
  FROM dbo.categories c
  INNER JOIN inserted i ON i.id = c.id;
END;
GO

CREATE OR ALTER TRIGGER trg_products_updated_at
ON dbo.products
AFTER UPDATE
AS
BEGIN
  SET NOCOUNT ON;
  UPDATE p SET updated_at = SYSUTCDATETIME()
  FROM dbo.products p
  INNER JOIN inserted i ON i.id = p.id;
END;
GO

CREATE OR ALTER TRIGGER trg_variants_updated_at
ON dbo.product_variants
AFTER UPDATE
AS
BEGIN
  SET NOCOUNT ON;
  UPDATE v SET updated_at = SYSUTCDATETIME()
  FROM dbo.product_variants v
  INNER JOIN inserted i ON i.id = v.id;
END;
GO

CREATE OR ALTER TRIGGER trg_inventories_updated_at
ON dbo.inventories
AFTER UPDATE
AS
BEGIN
  SET NOCOUNT ON;
  UPDATE inv SET updated_at = SYSUTCDATETIME()
  FROM dbo.inventories inv
  INNER JOIN inserted i ON i.id = inv.id;
END;
GO

CREATE OR ALTER TRIGGER trg_reviews_updated_at
ON dbo.product_reviews
AFTER UPDATE
AS
BEGIN
  SET NOCOUNT ON;
  UPDATE r SET updated_at = SYSUTCDATETIME()
  FROM dbo.product_reviews r
  INNER JOIN inserted i ON i.id = r.id;
END;
GO

CREATE OR ALTER TRIGGER trg_carts_updated_at
ON dbo.carts
AFTER UPDATE
AS
BEGIN
  SET NOCOUNT ON;
  UPDATE c SET updated_at = SYSUTCDATETIME()
  FROM dbo.carts c
  INNER JOIN inserted i ON i.id = c.id;
END;
GO

CREATE OR ALTER TRIGGER trg_cart_items_updated_at
ON dbo.cart_items
AFTER UPDATE
AS
BEGIN
  SET NOCOUNT ON;
  UPDATE ci SET updated_at = SYSUTCDATETIME()
  FROM dbo.cart_items ci
  INNER JOIN inserted i ON i.id = ci.id;
END;
GO

CREATE OR ALTER TRIGGER trg_orders_updated_at
ON dbo.orders
AFTER UPDATE
AS
BEGIN
  SET NOCOUNT ON;
  UPDATE o SET updated_at = SYSUTCDATETIME()
  FROM dbo.orders o
  INNER JOIN inserted i ON i.id = o.id;
END;
GO

CREATE OR ALTER TRIGGER trg_order_items_updated_at
ON dbo.order_items
AFTER UPDATE
AS
BEGIN
  SET NOCOUNT ON;
  UPDATE oi SET updated_at = SYSUTCDATETIME()
  FROM dbo.order_items oi
  INNER JOIN inserted i ON i.id = oi.id;
END;
GO


