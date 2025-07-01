using BitByBit.Entities.Enums;

public class RoomListResponseDto
{
    public int Id { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public RoomType Role { get; set; }
    public string RoomTypeName => Role.ToString();
    public decimal Price { get; set; }
    public int Capacity { get; set; }
    public bool Wifi { get; set; }
    public string? MainImageUrl { get; set; } // əsas şəkil üçün preview
}
