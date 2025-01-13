using System;
 using System.Numerics;
 using KodakkuAssist.Module.Draw;
 using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Script;
using Newtonsoft.Json;

namespace KodakkuScriptTea

{
    /// <summary>
    /// name and version affect the script name and version number displayed in the user interface.
    /// territorys specifies the regions where this trigger is effective. If left empty, it will be effective in all regions.
    /// Classes with the same GUID will be considered the same trigger. Please ensure your GUID is unique and does not conflict with others.
    /// </summary>
    [ScriptType(name: "绝亚P1.5地火起跑指路", territorys: [887],guid: "2bbc1449-ceab-4d61-90a6-a7d69e81da2f",version:"0.0.0.1",author: "Wotou")]
    public class TeaScript
    {
        private bool is1256 = false;
        private bool isDetermined = false;
        
        [ScriptMethod(name: "P1.5地火指路", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:18480"])]
        public void P15地火(Event @event, ScriptAccessory accessory)
        {
            if (@event.ActionId() != 18480) return;
            // var name = @event.SourceName();
            var pos = @event.EffectPosition();
            // accessory.Log.Debug("name: " + name);
            
            if (!isDetermined)
            {
                accessory.Method.SendChat("/e pos: " + pos);
                var side = DetermineSide(pos.X, pos.Z);
                
                if (side == "左侧" && is1256)
                {
                    //accessory.Method.SendChat("/e 去左侧");
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"地火起跑点";
                    dp.Scale = new(2);
                    dp.Owner = accessory.Data.Me;
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 2800;
                    dp.TargetPosition = pos;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                    isDetermined = true;
                }
                else if (side == "右侧" && !is1256)
                {
                    //accessory.Method.SendChat("/e 去右侧");
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"地火起跑点";
                    dp.Scale = new(2);
                    dp.Owner = accessory.Data.Me;
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 2800;
                    dp.TargetPosition = pos;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                    isDetermined = true;
                }
            }
        }
        
        [ScriptMethod(name: "P1.5麻将记录", eventType: EventTypeEnum.TargetIcon)]
        public void P15麻将记录(Event @event, ScriptAccessory accessory)
        {
             EventExtensions.ParseHexId(@event["Id"], out var id);
             isDetermined = false;
             var target = @event.TargetId();
             if (target != accessory.Data.Me) return;
             switch (id - 78)
             {
                 case 1:
                     is1256 = true;
                     break;
                 case 2:
                     is1256 = true;
                     break;
                 case 3:
                     is1256 = false;
                     break;
                 case 4:
                     is1256 = false;
                     break;
                 case 5:
                     is1256 = true;
                     break;
                case 6:
                    is1256 = true;
                    break;
                case 7:
                    is1256 = false;
                    break;
                case 8:
                    is1256 = false;
                    break;
             }
             accessory.Method.SendChat("/e 麻将: " + (id - 78));
             accessory.Method.SendChat("/e 是1256组吗: " + is1256);
        }
        
        // 判断传入点相对于偏转22.5度线的左右位置
        private static string DetermineSide(float x, float z)
        {
            // 基准点和正北方向参考点
            const float centerX = 100f;
            const float centerZ = 100f;
            const float northX = 100f;
            const float northZ = 0f;

            // 偏转22.5度的参考点
            const double angleOffset = 22.5; // 偏转角度
            double radiansOffset = angleOffset * Math.PI / 180; // 转为弧度

            // 偏转点的坐标 (计算正北点绕中心点顺时针偏转22.5度)
            double rotatedX = centerX + (northX - centerX) * Math.Cos(radiansOffset) - (northZ - centerZ) * Math.Sin(radiansOffset);
            double rotatedZ = centerZ + (northX - centerX) * Math.Sin(radiansOffset) + (northZ - centerZ) * Math.Cos(radiansOffset);

            // 计算传入点和中心点之间的向量
            float dx = x - centerX;
            float dz = z - centerZ;

            // 计算偏转点和中心点之间的向量
            double rotatedDx = rotatedX - centerX;
            double rotatedDz = rotatedZ - centerZ;

            // 使用叉积判断左右位置
            // 叉积 > 0：点在参考线的左侧，叉积 < 0：点在参考线的右侧
            double crossProduct = dx * rotatedDz - dz * rotatedDx;

            if (crossProduct > 0)
            {
                return "左侧";
            }
            else if (crossProduct < 0)
            {
                return "右侧";
            }
            else
            {
                return "在线上";
            }
        }
    }
}
 
public static class EventExtensions
{
    public static bool ParseHexId(string? idStr, out uint id)
    {
        id = 0;
        if (string.IsNullOrEmpty(idStr)) return false;
        try
        {
            var idStr2 = idStr.Replace("0x", "");
            id = uint.Parse(idStr2, System.Globalization.NumberStyles.HexNumber);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static uint ActionId(this Event @event)
    {
        return JsonConvert.DeserializeObject<uint>(@event["ActionId"]);
    }

    public static uint SourceId(this Event @event)
    {
        return ParseHexId(@event["SourceId"], out var id) ? id : 0;
    }


    public static string DurationMilliseconds(this Event @event)
    {
        return JsonConvert.DeserializeObject<string>(@event["DurationMilliseconds"]) ?? string.Empty;
    }

    public static uint SourceRotation(this Event @event)
    {
        return ParseHexId(@event["SourceRotation"], out var sourceRotation) ? sourceRotation : 0;
    }

    public static byte Index(this Event @event)
    {
        return (byte)(ParseHexId(@event["Index"], out var index) ? index : 0);
    }

    public static uint State(this Event @event)
    {
        return ParseHexId(@event["State"], out var state) ? state : 0;
    }

    public static string SourceName(this Event @event)
    {
        return JsonConvert.DeserializeObject<string>(@event["SourceName"]) ?? string.Empty;
    }

    public static uint TargetId(this Event @event)
    {
        return ParseHexId(@event["TargetId"], out var id) ? id : 0;
    }

    public static Vector3 SourcePosition(this Event @event)
    {
        return JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
    }

    public static Vector3 TargetPosition(this Event @event)
    {
        return JsonConvert.DeserializeObject<Vector3>(@event["TargetPosition"]);
    }

    public static Vector3 EffectPosition(this Event @event)
    {
        return JsonConvert.DeserializeObject<Vector3>(@event["EffectPosition"]);
    }
}
