namespace ChalmersxTools.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddLtiRequestToLtiSession : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.LtiSessions", "LtiRequest", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.LtiSessions", "LtiRequest");
        }
    }
}
