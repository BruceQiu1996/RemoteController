using System.ComponentModel.DataAnnotations;

namespace RemoteController.Common.Dtos.API.Equipment
{
    public class RegistDto
    {
        [Required]
        public string SessionId { get; set; }
        [Required]
        public string EquipmentId { get; set; }
        [Required]
        public string EquipmentSecret { get; set; }
        [Required]
        public ushort Port { get; set; }
    }
}
