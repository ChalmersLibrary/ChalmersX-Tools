namespace ChalmersxTools.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddLocationSubmissions : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.LocationSubmissions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(maxLength: 64),
                        ContextId = c.String(maxLength: 128),
                        Name = c.String(),
                        LocationName = c.String(),
                        LocationLat = c.Double(nullable: false),
                        LocationLong = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => new { t.UserId, t.ContextId }, unique: true, name: "IX_UserInContext");
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.LocationSubmissions", "IX_UserInContext");
            DropTable("dbo.LocationSubmissions");
        }
    }
}
