// MatchSettings는 현재 게임이 멀티 모드인지 봇 대전 모드인지 기억하는 정적 클래스입니다.
// 봇 대전은 네트워크 없이(GameMode.Single) 한 컴퓨터에서만 돌아가므로, 정적 값으로 충분합니다.
public enum MatchMode { Multi, Bot }

public static class MatchSettings
{
    public static MatchMode Mode = MatchMode.Multi;

    public static bool IsBotMatch => Mode == MatchMode.Bot;

    // 봇 대전에서 생성할 AI 수 (1:1 대전)
    public const int BotCount = 1;

    public const string BotNickname = "AI";
}
