using System.Text.Json.Serialization;

namespace TaskManagerApi.Models
{
    //هاد الملف بيحدد القيم المسموحة لحقلين بالمهمة: الحالة (Status) والأولوية (Priority).

    // Enum: نوع إله قيم محددة وثابتة بس، ما بتقدر تحط غيرها
    // JsonConverter: بالـ JSON بيطلع كنص ("Done") مش رقم (2)
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TaskItemStatus
    {
        Todo,       //المهمة يا لسا ما بلشت
        InProgress, //يا قيد التنفيذ
        Done        // جهزت
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TaskPriority
    {
        Low,    // منخفضة
        Medium, // متوسطة
        High    // عالية
    }
}
