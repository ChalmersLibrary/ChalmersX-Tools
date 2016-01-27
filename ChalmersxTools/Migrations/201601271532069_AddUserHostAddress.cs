namespace ChalmersxTools.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUserHostAddress : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.LtiSessions", "UserHostAddress", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.LtiSessions", "UserHostAddress");
        }
    }
}
