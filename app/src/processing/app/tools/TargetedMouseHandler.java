/**
 * @(#)MenuScroller.java	1.5.0 04/02/12
 */
package processing.app.tools;
import processing.app.debug.*;
import processing.app.syntax.*;
import processing.app.tools.*;
import processing.core.*;
import static processing.app.I18n._;

import java.awt.*;
import java.awt.datatransfer.*;
import java.awt.event.*;
import java.awt.print.*;
import java.io.*;
import java.net.*;
import java.util.*;
import java.util.zip.*;

import javax.swing.*;
import javax.swing.event.*;
import javax.swing.text.*;
import javax.swing.undo.*;

import gnu.io.*;
public class TargetedMouseHandler implements AWTEventListener
{

    private Component parent;
    //private Component innerBound;
    private boolean hasExited = true;

    public TargetedMouseHandler(Component p)
    {
        parent = p;
        //innerBound = p2;
    }

    @Override
    public void eventDispatched(AWTEvent e)
    {
    	System.out.println("Entered");
    	parent.invalidate();
        parent.validate();
        /*if (e instanceof MouseEvent)
        {
            if (SwingUtilities.isDescendingFrom(
                (Component) e.getSource(), parent))
            {
                MouseEvent m = (MouseEvent) e;
                if (m.getID() == MouseEvent.MOUSE_ENTERED)
                {
                    if (hasExited)
                    {
                    	parent.invalidate();
                    	parent.validate();
                        System.out.println("Entered");
                        hasExited = false;
                    }
                } else if (m.getID() == MouseEvent.MOUSE_EXITED)
                {
                    Point p = SwingUtilities.convertPoint(
                        (Component) e.getSource(),
                        m.getPoint(),
                        parent);
                    if (!parent.getBounds().contains(p))
                    {
                    	parent.invalidate();
                    	parent.validate();
                        System.out.println("Exited");
                        hasExited = true;
                    }
                }
            }
        }*/
    }
}