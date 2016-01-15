namespace ChalmersxTools.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddLocationName : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.StudentPresentations", "LocationName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.StudentPresentations", "LocationName");
        }
    }
}
