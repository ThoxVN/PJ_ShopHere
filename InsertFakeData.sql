/* =========================================================
   8) SEED DATA TỐI THIỂU ĐỂ CHẠY WEB
   - Users + Addresses
   - Categories
   - Products
   - Product Variants
   - Inventories
   ========================================================= */
SET NOCOUNT ON;

BEGIN TRY
    BEGIN TRAN;

    /* ------------------------------------------
       A) USERS: 1 admin + 5 customers
       ------------------------------------------ */
    INSERT INTO dbo.users (email, password_hash, full_name, phone, role, status)
    VALUES
        (N'admin@shophere.vn', N'bcrypt$admin_demo', N'ShopHere Admin', N'0900000000', 'admin', 'active'),
        (N'customer1@example.com', N'bcrypt$cust1', N'Customer One',   N'0900000001', 'customer', 'active'),
        (N'customer2@example.com', N'bcrypt$cust2', N'Customer Two',   N'0900000002', 'customer', 'active'),
        (N'customer3@example.com', N'bcrypt$cust3', N'Customer Three', N'0900000003', 'customer', 'active'),
        (N'customer4@example.com', N'bcrypt$cust4', N'Customer Four',  N'0900000004', 'customer', 'active'),
        (N'customer5@example.com', N'bcrypt$cust5', N'Customer Five',  N'0900000005', 'customer', 'active');

    /* ------------------------------------------
       B) ADDRESSES: mỗi customer 1 shipping + 1 billing
       ------------------------------------------ */
    ;WITH cust AS (
        SELECT
            userid,
            full_name,
            phone,
            ROW_NUMBER() OVER (ORDER BY userid) AS rn
        FROM dbo.users
        WHERE role = 'customer'
    )
    INSERT INTO dbo.addresses
        (user_id, type, recipient_name, phone, street, ward, district, city, postal_code, country, is_default)
    SELECT
        c.userid,
        'shipping',
        c.full_name,
        c.phone,
        CONCAT(N'Số ', c.rn, N' Đường Demo A'),
        N'Phường Demo',
        N'Quận Demo',
        N'Hồ Chí Minh',
        CONCAT('7000', c.rn),
        N'VN',
        1
    FROM cust c;

    ;WITH cust2 AS (
        SELECT
            userid,
            full_name,
            phone,
            ROW_NUMBER() OVER (ORDER BY userid) AS rn
        FROM dbo.users
        WHERE role = 'customer'
    )
    INSERT INTO dbo.addresses
        (user_id, type, recipient_name, phone, street, ward, district, city, postal_code, country, is_default)
    SELECT
        c.userid,
        'billing',
        c.full_name,
        c.phone,
        CONCAT(N'Số ', 100 + c.rn, N' Đường Demo B'),
        N'Phường Demo',
        N'Quận Demo',
        N'Hồ Chí Minh',
        CONCAT('7001', c.rn),
        N'VN',
        0
    FROM cust2 c;

    /* ------------------------------------------
       C) CATEGORIES: vài category top + sub
       ------------------------------------------ */
    -- Top-level
    INSERT INTO dbo.categories (name, slug, parent_id, is_active)
    VALUES
        (N'Fashion',    N'fashion',    NULL, 1),
        (N'Accessories',N'accessories',NULL, 1),
        (N'Shoes',      N'shoes',      NULL, 1);

    -- Sub cho Fashion
    INSERT INTO dbo.categories (name, slug, parent_id, is_active)
    SELECT N'Men',    N'men',    id, 1 FROM dbo.categories WHERE slug = N'fashion'
    UNION ALL
    SELECT N'Women',  N'women',  id, 1 FROM dbo.categories WHERE slug = N'fashion'
    UNION ALL
    SELECT N'Kids',   N'kids',   id, 1 FROM dbo.categories WHERE slug = N'fashion';

    /* ------------------------------------------
       D) PRODUCTS: ~10 products demo
       ------------------------------------------ */
    DECLARE @Products TABLE (
        category_slug NVARCHAR(255),
        sku           VARCHAR(255),
        name          NVARCHAR(255),
        material      NVARCHAR(255),
        description_html NVARCHAR(MAX)
    );

    INSERT INTO @Products (category_slug, sku, name, material, description_html)
    VALUES
        -- Men
        (N'men',   'MEN-TS-001', N'Cotton T-Shirt Classic', N'Cotton',    N'<p>Men classic cotton tee.</p>'),
        (N'men',   'MEN-SH-001', N'Oxford Shirt',           N'Cotton',    N'<p>Men oxford shirt for office.</p>'),
        (N'men',   'MEN-JE-001', N'Straight Jeans',         N'Denim',     N'<p>Men straight jeans.</p>'),
        -- Women
        (N'women', 'WMN-DR-001', N'Midi Dress',             N'Poly Blend',N'<p>Women elegant midi dress.</p>'),
        (N'women', 'WMN-TS-001', N'Ribbed Tee',             N'Cotton',    N'<p>Women ribbed tee.</p>'),
        (N'women', 'WMN-SK-001', N'Pleated Skirt',          N'Polyester', N'<p>Women pleated skirt.</p>'),
        -- Kids
        (N'kids',  'KID-TS-001', N'Kids Graphic Tee',       N'Cotton',    N'<p>Kids fun graphic tee.</p>'),
        (N'kids',  'KID-SH-001', N'Kids Shorts',            N'Cotton',    N'<p>Kids comfortable shorts.</p>'),
        -- Accessories
        (N'accessories','ACC-HT-001', N'Cotton Cap',        N'Cotton',    N'<p>Unisex cotton cap.</p>'),
        (N'accessories','ACC-BT-001', N'Leather Belt',      N'Leather',   N'<p>Genuine leather belt.</p>'),
        -- Shoes
        (N'shoes','SHO-SN-001', N'Sneakers Daily',          N'Synthetic', N'<p>Everyday sneakers.</p>'),
        (N'shoes','SHO-RN-001', N'Running Shoes',           N'Mesh',      N'<p>Lightweight running shoes.</p>');

    INSERT INTO dbo.products (category_id, sku, name, description_html, material, is_active)
    SELECT
        c.id,
        p.sku,
        p.name,
        p.description_html,
        p.material,
        1
    FROM @Products p
    JOIN dbo.categories c ON c.slug = p.category_slug;

    /* ------------------------------------------
       E) PRODUCT_VARIANTS:
          - Men/Women/Kids: size S/M/L, màu Black/White
          - Accessories: One Size, màu Black
          - Shoes: size 40/41/42, màu White
       ------------------------------------------ */
    -- Apparel variants (men, women, kids)
    ;WITH apparel AS (
        SELECT pr.id AS product_id, pr.sku
        FROM dbo.products pr
        JOIN dbo.categories c ON c.id = pr.category_id
        WHERE c.slug IN (N'men', N'women', N'kids')
    ),
    apparel_sizes AS (
        SELECT 'S' AS size_tag UNION ALL
        SELECT 'M' UNION ALL
        SELECT 'L'
    ),
    apparel_colors AS (
        SELECT N'Black' AS color, 'BLK' AS code
        UNION ALL
        SELECT N'White', 'WHT'
    )
    INSERT INTO dbo.product_variants
        (product_id, sku, name_extension, size, color, price_modifier, is_active)
    SELECT
        a.product_id,
        CONCAT(a.sku, '-', s.size_tag, '-', col.code),
        CONCAT(N'Size ', s.size_tag, N' / ', col.color),
        s.size_tag,
        col.color,
        0,          -- không cộng thêm giá
        1
    FROM apparel a
    CROSS JOIN apparel_sizes s
    CROSS JOIN apparel_colors col;

    -- Accessories: One Size / Black
    ;WITH acc AS (
        SELECT pr.id AS product_id, pr.sku
        FROM dbo.products pr
        JOIN dbo.categories c ON c.id = pr.category_id
        WHERE c.slug = N'accessories'
    )
    INSERT INTO dbo.product_variants
        (product_id, sku, name_extension, size, color, price_modifier, is_active)
    SELECT
        a.product_id,
        CONCAT(a.sku, '-OS-BLK'),
        N'One Size / Black',
        'One Size',
        N'Black',
        0,
        1
    FROM acc a;

    -- Shoes: size 40/41/42, màu White
    ;WITH shoes AS (
        SELECT pr.id AS product_id, pr.sku
        FROM dbo.products pr
        JOIN dbo.categories c ON c.id = pr.category_id
        WHERE c.slug = N'shoes'
    ),
    shoe_sizes AS (
        SELECT '40' AS size_tag UNION ALL
        SELECT '41' UNION ALL
        SELECT '42'
    )
    INSERT INTO dbo.product_variants
        (product_id, sku, name_extension, size, color, price_modifier, is_active)
    SELECT
        s.product_id,
        CONCAT(s.sku, '-', sz.size_tag, '-WHT'),
        CONCAT(N'Size ', sz.size_tag, N' / White'),
        sz.size_tag,
        N'White',
        0,
        1
    FROM shoes s
    CROSS JOIN shoe_sizes sz;

    /* ------------------------------------------
       F) INVENTORIES: mỗi variant có 50 tồn, 0 reserved
       ------------------------------------------ */
    INSERT INTO dbo.inventories (variant_id, qty_on_hand, qty_reserved)
    SELECT
        v.id,
        50,  -- on hand
        0    -- reserved
    FROM dbo.product_variants v;

    COMMIT;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK;
    THROW;
END CATCH;

PRINT 'Minimal seed data inserted successfully.';

/* Kiểm tra nhanh */
SELECT 'users' AS tbl, COUNT(*) AS cnt FROM dbo.users
UNION ALL SELECT 'addresses', COUNT(*) FROM dbo.addresses
UNION ALL SELECT 'categories', COUNT(*) FROM dbo.categories
UNION ALL SELECT 'products', COUNT(*) FROM dbo.products
UNION ALL SELECT 'product_variants', COUNT(*) FROM dbo.product_variants
UNION ALL SELECT 'inventories', COUNT(*) FROM dbo.inventories;
