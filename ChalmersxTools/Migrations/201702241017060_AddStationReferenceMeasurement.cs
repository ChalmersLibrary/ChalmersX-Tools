namespace ChalmersxTools.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddStationReferenceMeasurement : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SingleTemperatureMeasurementSubmissions", "StationPosition_Latitude", c => c.Double(nullable: false));
            AddColumn("dbo.SingleTemperatureMeasurementSubmissions", "StationPosition_Longitude", c => c.Double(nullable: false));
            AddColumn("dbo.SingleTemperatureMeasurementSubmissions", "StationTime", c => c.DateTime(nullable: false));
            AddColumn("dbo.SingleTemperatureMeasurementSubmissions", "StationMeasurement", c => c.Double());
            AddColumn("dbo.SingleTemperatureMeasurementSubmissions", "DistanceInMeters", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.SingleTemperatureMeasurementSubmissions", "DistanceInMeters");
            DropColumn("dbo.SingleTemperatureMeasurementSubmissions", "StationMeasurement");
            DropColumn("dbo.SingleTemperatureMeasurementSubmissions", "StationTime");
            DropColumn("dbo.SingleTemperatureMeasurementSubmissions", "StationPosition_Longitude");
            DropColumn("dbo.SingleTemperatureMeasurementSubmissions", "StationPosition_Latitude");
        }
    }
}
