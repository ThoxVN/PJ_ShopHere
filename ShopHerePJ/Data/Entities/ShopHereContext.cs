using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ShopHerePJ.Data.Entities;

public partial class ShopHereContext : DbContext
{
    public ShopHereContext()
    {
    }

    public ShopHereContext(DbContextOptions<ShopHereContext> options)
        : base(options)
    {
    }

    public virtual DbSet<address> addresses { get; set; }

    public virtual DbSet<cart> carts { get; set; }

    public virtual DbSet<cart_item> cart_items { get; set; }

    public virtual DbSet<category> categories { get; set; }

    public virtual DbSet<inventory> inventories { get; set; }

    public virtual DbSet<order> orders { get; set; }

    public virtual DbSet<order_item> order_items { get; set; }

    public virtual DbSet<product> products { get; set; }

    public virtual DbSet<product_review> product_reviews { get; set; }

    public virtual DbSet<product_variant> product_variants { get; set; }

    public virtual DbSet<user> users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=THOX\\SQLEXPRESS;Database=shophere;User Id=sa;Password=1;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<address>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__addresse__3213E83F5AF63A2E");

            entity.ToTable(tb => tb.HasTrigger("trg_addresses_updated_at"));

            entity.Property(e => e.city).HasMaxLength(255);
            entity.Property(e => e.country)
                .HasMaxLength(50)
                .HasDefaultValue("VN");
            entity.Property(e => e.created_at)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.district).HasMaxLength(255);
            entity.Property(e => e.phone).HasMaxLength(50);
            entity.Property(e => e.postal_code).HasMaxLength(50);
            entity.Property(e => e.recipient_name).HasMaxLength(255);
            entity.Property(e => e.street).HasMaxLength(255);
            entity.Property(e => e.type)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.updated_at)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.ward).HasMaxLength(255);

            entity.HasOne(d => d.user).WithMany(p => p.addresses)
                .HasForeignKey(d => d.user_id)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_addresses_users");
        });

        modelBuilder.Entity<cart>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__carts__3213E83F67C58015");

            entity.ToTable(tb => tb.HasTrigger("trg_carts_updated_at"));

            entity.HasIndex(e => e.status, "IX_carts_status");

            entity.HasIndex(e => e.user_id, "IX_carts_user");

            entity.HasIndex(e => e.session_id, "UQ__carts__69B13FDDD26BD2EF").IsUnique();

            entity.Property(e => e.created_at)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.currency)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValue("VND");
            entity.Property(e => e.discount_amount).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.grand_total).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.session_id)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.shipping_fee).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("active");
            entity.Property(e => e.subtotal_amount).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.tax_amount).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.updated_at)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.user).WithMany(p => p.carts)
                .HasForeignKey(d => d.user_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_carts_user");
        });

        modelBuilder.Entity<cart_item>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__cart_ite__3213E83F4DE84DEC");

            entity.ToTable(tb => tb.HasTrigger("trg_cart_items_updated_at"));

            entity.HasIndex(e => e.cart_id, "IX_cart_items_cart");

            entity.HasIndex(e => new { e.cart_id, e.variant_id }, "UQ_cart_item_one_variant").IsUnique();

            entity.Property(e => e.created_at)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.line_discount_amount).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.line_total).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.unit_price).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.updated_at)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.cart).WithMany(p => p.cart_items)
                .HasForeignKey(d => d.cart_id)
                .HasConstraintName("FK_cart_items_cart");

            entity.HasOne(d => d.variant).WithMany(p => p.cart_items)
                .HasForeignKey(d => d.variant_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_cart_items_variant");
        });

        modelBuilder.Entity<category>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__categori__3213E83F8AFE10B6");

            entity.ToTable(tb => tb.HasTrigger("trg_categories_updated_at"));

            entity.HasIndex(e => e.name, "IX_categories_name");

            entity.HasIndex(e => e.slug, "UQ__categori__32DD1E4CE743D943").IsUnique();

            entity.Property(e => e.created_at)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.is_active).HasDefaultValue(true);
            entity.Property(e => e.name).HasMaxLength(255);
            entity.Property(e => e.slug).HasMaxLength(255);
            entity.Property(e => e.updated_at)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.parent).WithMany(p => p.Inverseparent)
                .HasForeignKey(d => d.parent_id)
                .HasConstraintName("FK_categories_parent");
        });

        modelBuilder.Entity<inventory>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__inventor__3213E83FDE0581BD");

            entity.ToTable(tb => tb.HasTrigger("trg_inventories_updated_at"));

            entity.HasIndex(e => e.variant_id, "UQ__inventor__EACC68B64FAA539C").IsUnique();

            entity.Property(e => e.updated_at)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.variant).WithOne(p => p.inventory)
                .HasForeignKey<inventory>(d => d.variant_id)
                .HasConstraintName("FK_inventories_variant");
        });

        modelBuilder.Entity<order>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__orders__3213E83F2DC1A51E");

            entity.ToTable(tb => tb.HasTrigger("trg_orders_updated_at"));

            entity.HasIndex(e => e.status, "IX_orders_status");

            entity.HasIndex(e => e.user_id, "IX_orders_user");

            entity.HasIndex(e => e.order_number, "UQ__orders__730E34DF4EA16588").IsUnique();

            entity.Property(e => e.created_at)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.currency)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValue("VND");
            entity.Property(e => e.delivered_at).HasPrecision(0);
            entity.Property(e => e.discount_amount).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.grand_total).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.order_number)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.paid_at).HasPrecision(0);
            entity.Property(e => e.payment_method)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.payment_status)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("unpaid");
            entity.Property(e => e.shipped_at).HasPrecision(0);
            entity.Property(e => e.shipping_fee).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.shipping_method)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.status)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("pending_payment");
            entity.Property(e => e.subtotal_amount).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.tax_amount).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.tracking_number).HasMaxLength(100);
            entity.Property(e => e.updated_at)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.billing_address).WithMany(p => p.orderbilling_addresses)
                .HasForeignKey(d => d.billing_address_id)
                .HasConstraintName("FK_orders_bill_address");

            entity.HasOne(d => d.cart).WithMany(p => p.orders)
                .HasForeignKey(d => d.cart_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_orders_cart");

            entity.HasOne(d => d.shipping_address).WithMany(p => p.ordershipping_addresses)
                .HasForeignKey(d => d.shipping_address_id)
                .HasConstraintName("FK_orders_ship_address");

            entity.HasOne(d => d.user).WithMany(p => p.orders)
                .HasForeignKey(d => d.user_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_orders_user");
        });

        modelBuilder.Entity<order_item>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__order_it__3213E83FD82BDD85");

            entity.ToTable(tb => tb.HasTrigger("trg_order_items_updated_at"));

            entity.HasIndex(e => e.order_id, "IX_order_items_order");

            entity.Property(e => e.created_at)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.line_discount_amount).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.line_total).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.product_name_snapshot).HasMaxLength(255);
            entity.Property(e => e.sku_snapshot)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.unit_price).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.updated_at)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.variant_name_snapshot).HasMaxLength(255);

            entity.HasOne(d => d.order).WithMany(p => p.order_items)
                .HasForeignKey(d => d.order_id)
                .HasConstraintName("FK_order_items_order");

            entity.HasOne(d => d.variant).WithMany(p => p.order_items)
                .HasForeignKey(d => d.variant_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_order_items_variant");
        });

        modelBuilder.Entity<product>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__products__3213E83FB6D4DFC0");

            entity.ToTable(tb => tb.HasTrigger("trg_products_updated_at"));

            entity.HasIndex(e => e.name, "IX_products_name");

            entity.HasIndex(e => e.sku, "UQ__products__DDDF4BE7976F47AE").IsUnique();

            entity.Property(e => e.created_at)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.is_active).HasDefaultValue(true);
            entity.Property(e => e.material).HasMaxLength(255);
            entity.Property(e => e.name).HasMaxLength(255);
            entity.Property(e => e.sku)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.updated_at)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.category).WithMany(p => p.products)
                .HasForeignKey(d => d.category_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_products_categories");
        });

        modelBuilder.Entity<product_review>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__product___3213E83F126197FA");

            entity.ToTable(tb => tb.HasTrigger("trg_reviews_updated_at"));

            entity.HasIndex(e => e.product_id, "IX_reviews_product");

            entity.HasIndex(e => e.user_id, "IX_reviews_user");

            entity.HasIndex(e => new { e.product_id, e.user_id }, "UQ_reviews_product_user").IsUnique();

            entity.Property(e => e.created_at)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.title).HasMaxLength(255);
            entity.Property(e => e.updated_at)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.product).WithMany(p => p.product_reviews)
                .HasForeignKey(d => d.product_id)
                .HasConstraintName("FK_reviews_product");

            entity.HasOne(d => d.user).WithMany(p => p.product_reviews)
                .HasForeignKey(d => d.user_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_reviews_user");
        });

        modelBuilder.Entity<product_variant>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__product___3213E83F0C32A26B");

            entity.ToTable(tb => tb.HasTrigger("trg_variants_updated_at"));

            entity.HasIndex(e => e.product_id, "IX_variants_product");

            entity.HasIndex(e => e.sku, "UQ__product___DDDF4BE75BBBE8B0").IsUnique();

            entity.HasIndex(e => new { e.product_id, e.size, e.color }, "UQ_variants_product_size_color").IsUnique();

            entity.Property(e => e.color).HasMaxLength(50);
            entity.Property(e => e.created_at)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.is_active).HasDefaultValue(true);
            entity.Property(e => e.name_extension).HasMaxLength(255);
            entity.Property(e => e.price_modifier).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.size)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.sku)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.updated_at)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.product).WithMany(p => p.product_variants)
                .HasForeignKey(d => d.product_id)
                .HasConstraintName("FK_variants_product");
        });

        modelBuilder.Entity<user>(entity =>
        {
            entity.HasKey(e => e.userid).HasName("PK__users__CBA1B2574235D496");

            entity.ToTable(tb => tb.HasTrigger("trg_users_updated_at"));

            entity.HasIndex(e => e.email, "UQ__users__AB6E61648B445812").IsUnique();

            entity.Property(e => e.created_at)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.email).HasMaxLength(255);
            entity.Property(e => e.full_name).HasMaxLength(255);
            entity.Property(e => e.password_hash).HasMaxLength(255);
            entity.Property(e => e.phone).HasMaxLength(50);
            entity.Property(e => e.role)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("customer");
            entity.Property(e => e.status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("active");
            entity.Property(e => e.updated_at)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
