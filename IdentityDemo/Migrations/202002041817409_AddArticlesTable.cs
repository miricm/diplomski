namespace IdentityDemo.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddArticlesTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Articles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        Category = c.String(),
                        Text = c.String(),
                        Image = c.Binary(),
                        DatePublished = c.DateTime(nullable: false),
                        Author_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.Author_Id)
                .Index(t => t.Author_Id);
            
            CreateTable(
                "dbo.Comments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Text = c.String(),
                        DatePublished = c.DateTime(nullable: false),
                        Article_Id = c.Int(),
                        PostedBy_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Articles", t => t.Article_Id)
                .ForeignKey("dbo.AspNetUsers", t => t.PostedBy_Id)
                .Index(t => t.Article_Id)
                .Index(t => t.PostedBy_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Articles", "Author_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.Comments", "PostedBy_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.Comments", "Article_Id", "dbo.Articles");
            DropIndex("dbo.Comments", new[] { "PostedBy_Id" });
            DropIndex("dbo.Comments", new[] { "Article_Id" });
            DropIndex("dbo.Articles", new[] { "Author_Id" });
            DropTable("dbo.Comments");
            DropTable("dbo.Articles");
        }
    }
}
