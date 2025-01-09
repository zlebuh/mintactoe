namespace Zlebuh.MinTacToe.Services
{
    internal interface IMineAuthority
    {
        bool IsMine(bool considerNoMineMovesRule);
    }
}
