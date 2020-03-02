namespace IdentityDemo.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDateRegisteredColumn : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "DateRegistered", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "DateRegistered");
        }
    }
}
