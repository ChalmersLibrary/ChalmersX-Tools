namespace ChalmersxTools.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddEarthMassV2Submission : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.EarthMassV2Submission",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(maxLength: 64),
                        ContextId = c.String(maxLength: 128),
                        MeanGravityAcceleration = c.Double(nullable: false),
                        TotalEarthMass = c.Double(nullable: false),
                        Location = c.String(),
                        Position_Latitude = c.Double(nullable: false),
                        Position_Longitude = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => new { t.UserId, t.ContextId }, unique: true, name: "IX_UserInContext");
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.EarthMassV2Submission", "IX_UserInContext");
            DropTable("dbo.EarthMassV2Submission");
        }
    }
}
