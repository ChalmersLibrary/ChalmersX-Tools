namespace ChalmersxTools.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCoordinateEarthSpheresImages : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EarthSpheresImagesSubmissions", "Sphere1Location", c => c.String());
            AddColumn("dbo.EarthSpheresImagesSubmissions", "Sphere1Coordinate_Latitude", c => c.Double(nullable: false));
            AddColumn("dbo.EarthSpheresImagesSubmissions", "Sphere1Coordinate_Longitude", c => c.Double(nullable: false));
            AddColumn("dbo.EarthSpheresImagesSubmissions", "Sphere2Location", c => c.String());
            AddColumn("dbo.EarthSpheresImagesSubmissions", "Sphere2Coordinate_Latitude", c => c.Double(nullable: false));
            AddColumn("dbo.EarthSpheresImagesSubmissions", "Sphere2Coordinate_Longitude", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.EarthSpheresImagesSubmissions", "Sphere2Coordinate_Longitude");
            DropColumn("dbo.EarthSpheresImagesSubmissions", "Sphere2Coordinate_Latitude");
            DropColumn("dbo.EarthSpheresImagesSubmissions", "Sphere2Location");
            DropColumn("dbo.EarthSpheresImagesSubmissions", "Sphere1Coordinate_Longitude");
            DropColumn("dbo.EarthSpheresImagesSubmissions", "Sphere1Coordinate_Latitude");
            DropColumn("dbo.EarthSpheresImagesSubmissions", "Sphere1Location");
        }
    }
}
