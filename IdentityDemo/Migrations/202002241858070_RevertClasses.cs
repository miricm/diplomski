namespace IdentityDemo.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RevertClasses : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Articles", "AuthorUser_Id", "dbo.AspNetUsers");
            DropIndex("dbo.Articles", new[] { "AuthorUser_Id" });
            DropColumn("dbo.Articles", "AuthorUser_Id");
            DropColumn("dbo.AspNetUsers", "Discriminator");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "Discriminator", c => c.String(nullable: false, maxLength: 128));
            AddColumn("dbo.Articles", "AuthorUser_Id", c => c.String(maxLength: 128));
            CreateIndex("dbo.Articles", "AuthorUser_Id");
            AddForeignKey("dbo.Articles", "AuthorUser_Id", "dbo.AspNetUsers", "Id");
        }
    }
}
