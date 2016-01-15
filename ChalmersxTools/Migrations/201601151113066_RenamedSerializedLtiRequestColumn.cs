namespace ChalmersxTools.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenamedSerializedLtiRequestColumn : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.LtiSessions", "LtiRequestSerialized", c => c.String());
            DropColumn("dbo.LtiSessions", "LtiRequest");
        }
        
        public override void Down()
        {
            AddColumn("dbo.LtiSessions", "LtiRequest", c => c.String());
            DropColumn("dbo.LtiSessions", "LtiRequestSerialized");
        }
    }
}
