namespace IdentityDemo.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCustomUserClasses : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Articles", "AuthorUser_Id", c => c.String(maxLength: 128));
            AddColumn("dbo.AspNetUsers", "Discriminator", c => c.String(nullable: false, maxLength: 128));
            CreateIndex("dbo.Articles", "AuthorUser_Id");
            AddForeignKey("dbo.Articles", "AuthorUser_Id", "dbo.AspNetUsers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Articles", "AuthorUser_Id", "dbo.AspNetUsers");
            DropIndex("dbo.Articles", new[] { "AuthorUser_Id" });
            DropColumn("dbo.AspNetUsers", "Discriminator");
            DropColumn("dbo.Articles", "AuthorUser_Id");
        }
    }
}
