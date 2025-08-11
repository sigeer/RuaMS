using Application.Utility;
using Application.Utility.Configs;

namespace Application.Shared.Events
{
    /**
     * @author Alan (SharpAceX)
     */

    public class ExpeditionType : EnumClass
    {

        public static readonly ExpeditionType BALROG_EASY = new(3, 30, 50, 255, 5);
        public static readonly ExpeditionType BALROG_NORMAL = new(6, 30, 50, 255, 5);
        public static readonly ExpeditionType SCARGA = new(6, 30, 100, 255, 5);
        public static readonly ExpeditionType SHOWA = new(3, 30, 100, 255, 5);
        public static readonly ExpeditionType ZAKUM = new(6, 30, 50, 255, 5);
        public static readonly ExpeditionType HORNTAIL = new(6, 30, 100, 255, 5);
        public static readonly ExpeditionType CHAOS_ZAKUM = new(6, 30, 120, 255, 5);
        public static readonly ExpeditionType CHAOS_HORNTAIL = new(6, 30, 120, 255, 5);
        public static readonly ExpeditionType ARIANT = new(2, 7, 20, 30, 5);
        public static readonly ExpeditionType ARIANT1 = new(2, 7, 20, 30, 5);
        public static readonly ExpeditionType ARIANT2 = new(2, 7, 20, 30, 5);
        public static readonly ExpeditionType PINKBEAN = new(6, 30, 120, 255, 5);
        public static readonly ExpeditionType CWKPQ = new(6, 30, 90, 255, 5);   // CWKPQ min-level 90, found thanks to Cato

        private int minSize;
        private int maxSize;
        private int minLevel;
        private int maxLevel;
        private int registrationMinutes;

        ExpeditionType(int minSize, int maxSize, int minLevel, int maxLevel, int minutes)
        {
            this.minSize = minSize;
            this.maxSize = maxSize;
            this.minLevel = minLevel;
            this.maxLevel = maxLevel;
            this.registrationMinutes = minutes;
        }

        public int getMinSize()
        {
            return !YamlConfig.config.server.USE_ENABLE_SOLO_EXPEDITIONS ? minSize : 1;
        }

        public int getMaxSize()
        {
            return maxSize;
        }

        public int getMinLevel()
        {
            return minLevel;
        }

        public int getMaxLevel()
        {
            return maxLevel;
        }

        public int getRegistrationMinutes()
        {
            return registrationMinutes;
        }
    }
}
