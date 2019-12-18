using Constellation;
using UnityEditor;
using UnityEngine;

namespace ConstellationEditor
{
    public class LinkView
    {
        private ConstellationScript constellationScript;
        private NodeEditorPanel editor;
        private NodeConfig nodeConfig;
        private bool dragging;
        public delegate void LinkRemoved(LinkData link);
        LinkRemoved OnLinkRemoved;

        public LinkView(IGUI _gui, NodeEditorPanel _editor, ConstellationScript _constellationScript, NodeConfig _nodeConfig, LinkRemoved _onLinkRemoved)
        {
            constellationScript = _constellationScript;
            editor = _editor;
            nodeConfig = _nodeConfig;
            OnLinkRemoved += _onLinkRemoved;
        }

        public LinkData[] GetLinks()
        {
            return constellationScript.GetLinks();
        }

        public void DrawLinks()
        {
            foreach(var link in constellationScript.GetLinks())
            {
                var output = OutputPosition(link.Output);
                var input = InputPosition(link.Input);

                if(output == Rect.zero || input == Rect.zero)
                {
                    constellationScript.RemoveLink(link);
                    OnLinkRemoved(link);
                }

                DrawNodeCurve(output, input, nodeConfig.GetConnectionColor(link.Input.IsWarm, link.Input.Type));
                MouseOverLink(link, output, input);
            }
        }

        public Rect InputPosition(InputData _input)
        {
            foreach(var node in constellationScript.GetNodes())
            {
                var i = 1;
                foreach(var input in node.GetInputs())
                {
                    if(_input.Guid == input.Guid)
                    {
                        return new Rect
                        {
                            x = node.XPosition,
                            y = node.YPosition + Offset + ((nodeConfig.InputSize) * i)
                        };
                    }
                    i++;
                }
            }
            return Rect.zero;
        }

        public Rect OutputPosition(OutputData _output)
        {
            foreach(var node in constellationScript.GetNodes())
            {
                var j = 1;
                foreach(var output in node.GetOutputs())
                {
                    if(_output.Guid == output.Guid)
                    {
                        return new Rect
                        {
                            x = node.XPosition + nodeConfig.GetNodeWidth(node),
                            y = node.YPosition + Offset + (nodeConfig.InputSize * j)
                        };
                    }
                    j++;
                }
            }
            return Rect.zero;
        }

        public void DrawNodeCurve(Rect _output, Rect _input)
        {
            DrawNodeCurve(_output, _input, Color.gray);
        }

        public void DrawNodeCurve(Rect _ouput, Rect _input, Color _color)
        {
            var start = new Vector3(_ouput.x + _ouput.width, _ouput.y + (_ouput.height / 2), 0);
            var end = new Vector3(_input.x, _input.y + (_input.height / 2), 0);
            if(editor.InView(PointsToRect(start, end)))
            {
                var distance = Vector3.Distance(start, end);
                var startTan = start + (Vector3.right * distance * 0.5f);
                var endTan = end + (Vector3.left * distance * 0.5f);
                Handles.DrawBezier(start, end, startTan, endTan, _color, null, 5);
            }
        }

        private void MouseOverLink(LinkData _link, Rect _output, Rect _input)
        {
            if(MouseOverCurve(_output.position, _input.position))
            {
                var linkCenter = new Rect
                {
                    x = (_output.x + ((_input.x - _output.x) / 2)) - Offset,
                    y = (_output.y + ((_input.y - _output.y) / 2)) - Offset,
                    width = nodeConfig.LinkButtonSize,
                    height = nodeConfig.LinkButtonSize,
                };

                GUI.Box(linkCenter, "", nodeConfig.HexagonButton);
                GUI.Button(linkCenter, "", nodeConfig.CloseButton);

                if(Event.current.IsUsed())
                {
                    if(Event.current.button == 0 && !dragging)
                    {
                        dragging = true;
                        if(linkCenter.Contains(Event.current.mousePosition))
                        {
                            constellationScript.RemoveLink(_link);
                            OnLinkRemoved(_link);
                        }
                    }
                }
                else if(!Event.current.IsLayoutOrRepaint())
                {
                    dragging = false;
                }
            }
        }

        private bool MouseOverCurve(Vector3 _output, Vector3 _input)
        {
            //Currently creates rect to detect mouse over so it's nowhere near pixel perfect detection
            var mouse = Event.current.mousePosition;
            return InRange(mouse.x, _output.x, _input.x) && InRange(mouse.y, _output.y, _input.y, 10);
        }

        private bool InRange(float _value, float _start, float _end, float _padding = 0)
        {
            return _value + _padding > Mathf.Min(_start, _end) && _value - _padding < Mathf.Max(_start, _end);
        }

        private Rect PointsToRect(Vector3 _start, Vector3 _end)
        {
            return new Rect
            {
                x = Mathf.Min(_start.x, _end.x),
                y = Mathf.Min(_start.y, _end.y),
                width = Mathf.Abs(_start.x - _end.x),
                height = Mathf.Abs(_start.y - _end.y)
            };
        }

        private float Offset
        {
            get
            {
                return nodeConfig.TopMargin * 0.5f;
            }
        }
    }
}