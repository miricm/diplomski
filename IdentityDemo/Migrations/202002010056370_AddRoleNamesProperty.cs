namespace IdentityDemo.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRoleNamesProperty : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "RoleNames", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "RoleNames");
        }
    }
}
