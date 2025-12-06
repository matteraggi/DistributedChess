using Shared.Models;

namespace ChessBackend.Helper
{
    public static class GameHelper
    {
        public static void AssignShards(List<Player> teamMembers, Dictionary<string, List<char>> permissions)
        {
            if (teamMembers.Count == 0) return;

            if (teamMembers.Count == 1)
            {
                return;
            }

            //per 2 giocatori a team
            var p1 = teamMembers[0];
            var p2 = teamMembers[1];
            permissions[p1.PlayerId] = new List<char> { 'P', 'K' };
            permissions[p2.PlayerId] = new List<char> { 'R', 'N', 'B', 'Q' };

            // per più di 3? ... metodo per gestire in automatico
        }
    }
}
