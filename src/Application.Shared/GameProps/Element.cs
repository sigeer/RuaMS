using Application.Utility;

namespace Application.Shared.GameProps
{
    /// <summary>
    /// 元素（克制？）,物理，火毒冰雷圣暗
    /// </summary>
    public class Element : EnumClass
    {
        public readonly static Element NEUTRAL = new(0);
        public readonly static Element PHYSICAL = new(1);
        public readonly static Element FIRE = new(2, true);
        public readonly static Element ICE = new(3, true);
        public readonly static Element LIGHTING = new(4);
        public readonly static Element POISON = new(5);
        public readonly static Element HOLY = new(6, true);
        public readonly static Element DARKNESS = new(7);

        private int value;
        private bool special = false;
        private Element(int v)
        {
            this.value = v;
        }

        private Element(int v, bool special)
        {
            this.value = v;
            this.special = special;
        }

        public bool isSpecial()
        {
            return special;
        }

        public static Element getFromChar(char c)
        {
            switch (char.ToUpper(c))
            {
                case 'F':
                    return Element.FIRE;
                case 'I':
                    return Element.ICE;
                case 'L':
                    return Element.LIGHTING;
                case 'S':
                    return Element.POISON;
                case 'H':
                    return Element.HOLY;
                case 'D':
                    return Element.DARKNESS;
                case 'P':
                    return Element.NEUTRAL;
            }
            throw new Exception("unknown elemnt char " + c);
        }

        public int getValue()
        {
            return value;
        }

        public override string ToString()
        {
            return name();
        }
    }
}
