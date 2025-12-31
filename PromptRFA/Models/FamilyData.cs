// PromptRFA/Models/FamilyData.cs
using System.Collections.Generic;

namespace PromptRFA.Models
{
  // JSONのルート構造
  public class RootObject
  {
    public string? command {get; set;}
    public FamilyData? parameters{get; set;}
  }
  // 共通のファミリ構造
  public class FamilyData
  {
    public string? familyName{get; set;}
    public string? category{get; set;}
    public string? type{get; set;}
    public Object? specs{get; set;}
  }
}