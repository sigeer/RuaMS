using Application.Core.Game.Maps;
using server.minigame;
using static Application.Core.Game.Maps.MiniGame;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
        private MiniGame? miniGame;
        public MiniGame? getMiniGame()
        {
            return miniGame;
        }

        public void setMiniGame(MiniGame? miniGame)
        {
            this.miniGame = miniGame;
        }

        public int getMiniGamePoints(MiniGameResult type, bool omok)
        {
            if (omok)
            {
                switch (type)
                {
                    case MiniGameResult.WIN:
                        return Omokwins;
                    case MiniGameResult.LOSS:
                        return Omoklosses;
                    default:
                        return Omokties;
                }
            }
            else
            {
                switch (type)
                {
                    case MiniGameResult.WIN:
                        return Matchcardwins;
                    case MiniGameResult.LOSS:
                        return Matchcardlosses;
                    default:
                        return Matchcardties;
                }
            }
        }

        public void setMiniGamePoints(Player visitor, int winnerslot, bool omok)
        {
            if (omok)
            {
                if (winnerslot == 1)
                {
                    this.Omokwins++;
                    visitor.Omoklosses++;
                }
                else if (winnerslot == 2)
                {
                    visitor.Omokwins++;
                    this.Omoklosses++;
                }
                else
                {
                    this.Omokties++;
                    visitor.Omokties++;
                }
            }
            else
            {
                if (winnerslot == 1)
                {
                    this.Matchcardwins++;
                    visitor.Matchcardlosses++;
                }
                else if (winnerslot == 2)
                {
                    visitor.Matchcardwins++;
                    this.Matchcardlosses++;
                }
                else
                {
                    this.Matchcardties++;
                    visitor.Matchcardties++;
                }
            }
        }


        public void closeMiniGame(bool forceClose)
        {
            var game = this.getMiniGame();
            if (game == null)
            {
                return;
            }

            if (game.isOwner(this))
            {
                game.closeRoom(forceClose);
            }
            else
            {
                game.removeVisitor(forceClose, this);
            }
        }

        private RockPaperScissor? rps;
        public RockPaperScissor? getRPS()
        { // thanks inhyuk for suggesting RPS addition
            return rps;
        }
        public void setRPS(RockPaperScissor? rps)
        {
            this.rps = rps;
        }

        public void closeRPS()
        {
            RockPaperScissor? rps = this.rps;
            if (rps != null)
            {
                rps.dispose(Client);
                setRPS(null);
            }
        }
    }
}
