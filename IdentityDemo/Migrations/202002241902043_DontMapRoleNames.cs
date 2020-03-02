namespace IdentityDemo.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DontMapRoleNames : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.AspNetUsers", "RoleNames");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "RoleNames", c => c.String());
        }
    }
}
