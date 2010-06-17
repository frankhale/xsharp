// windowtest.cs
//
// This basically tests a bunch of the functions while doing some useful stuff.
//
// In a perfect world I wouldn't be importing System.Runtime.InteropServices in here.
// Things are still rough around the edges. We'll get there in time!
//
// Copyright (C) 2010
//
// Frank Hale <frankhale@gmail.com> aka majyk
//            <http://github.com/frankhale> @ GitHub
//
// irc.freenode.net - ##sandbox
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using System.Timers;
using XSharp;
using System.Runtime.InteropServices;

namespace XSharpTest
{
  public class WindowResizeTest
  {
    XDisplay dpy;
    XScreen s;
    XEvent ev;

    XColor bg_color;
    XColor handle_color;

    XGC gc;

    XWindow main_win, resize_main_win, resize_top_left_win, resize_top_right_win, resize_bottom_left_win, resize_bottom_right_win;

    XWindowAttributes attr;
    XPointer pointer;

    XButtonEvent start;

    int old_resize_x = 140;
    int old_resize_y = 100;

    int min_size = 50;
    int resize_win_x = 140;
    int resize_win_y = 100;
    int resize_win_width = 320;
    int resize_win_height = 240;
    int resize_handle_width = 30;
    int resize_handle_height = 5;

    //XAtom atom;

    //private XPixmap pix;
    //private string[] pix_data;

    Timer t;

    public WindowResizeTest ()
    {
      t = new Timer ();
      
      dpy = new XDisplay ();
      s = new XScreen (dpy);
      ev = new XEvent (dpy);
      pointer = new XPointer (dpy);
      gc = new XGC (dpy);

      //XShape shape = new XShape(dpy);
      //shape.Query();
      //Console.WriteLine("Shape Event Type Number = {0}", shape.Type.ToString());
      
      //atom = new XAtom (dpy, "_MAJYK_HINT", false);
      
      // Used to test the XPM function that reads a pixmap from a char**
//      pix_data = new string[] {
//            "16 16 16 1",
//            "   c None",
//            ". c #323232",
//            "+  c #535353",
//            "@  c #4A8A8E",
//            "#  c #DEE2E2",
//            "$  c #7E827A",
//            "%  c #8A9292",
//            "&  c #D6D6D6",
//            "*  c #36767E",
//            "=  c #9E9E9E",
//            "-  c #FAFAFA",
//            ";  c #B2B2B2",
//            ">  c #DEEEEA",
//            ",  c #464646",
//            "'  c #5EA2A2",
//            ")  c #52969A",
//            "                ",
//            "                ",
//            " --#>>>>>>#-#-; ",
//            " -&%')))))=&=&+ ",
//            " >;$@*****=;%;+ ",
//            " &$$$$$$$$$$$$, ",
//            " &;;;;;;;;;;;;+ ",
//            " &;;;;;;;;;;;;+ ",
//            " #;;;;;;;;;;;;+ ",
//            " &;;;;;;;;;;;;+ ",
//            " #;;;;;;;;;;;;+ ",
//            " #;;;;;;;;;;;;+ ",
//            " &;;;;;;;;;;;;+ ",
//            " $............. ",
//            "                ",
//            "                "
//      };

      bg_color = new XColor (dpy, "#AAAAAA");
      handle_color = new XColor (dpy, "#FF0000");

      //bg_color = new XColor(dpy, Color.FromArgb(200, 200, 200));
      //handle_color = new XColor(dpy, Color.FromArgb(0, 255, 0));

      main_win = new XWindow (dpy, new Rectangle (5, 5, 640, 480), 0, s.BlackPixel (), s.WhitePixel ());

      main_win.Name = "Window Resize Test";
      main_win.SelectInput (XEventMask.KeyPressMask | XEventMask.ExposureMask);

      resize_main_win = new XWindow (dpy, main_win, new Rectangle (resize_win_x, resize_win_y, resize_win_width, resize_win_height), 1, s.BlackPixel (), bg_color.Pixel);
      resize_top_left_win = new XWindow (dpy, resize_main_win, new Rectangle (0, 0, resize_handle_width, resize_handle_height), 1, s.BlackPixel (), handle_color.Pixel);
      resize_top_right_win = new XWindow (dpy, resize_main_win, new Rectangle (0, 0, resize_handle_width, resize_handle_height), 1, s.BlackPixel (), handle_color.Pixel);
      resize_bottom_left_win = new XWindow (dpy, resize_main_win, new Rectangle (0, 0, resize_handle_width, resize_handle_height), 1, s.BlackPixel (), handle_color.Pixel);
      resize_bottom_right_win = new XWindow (dpy, resize_main_win, new Rectangle (0, 0, resize_handle_width, resize_handle_height), 1, s.BlackPixel (), handle_color.Pixel);

      //string foo="MAJYK!";
      //resize_main_win.ChangeProperty(atom, atom, 8, PropMode.PropModeReplace, Marshal.StringToHGlobalAnsi(foo), foo.Length);

      //int return_type=0;
      //int actual_format=0;
      //int nitems=0;
      //int bytes_return=0;
      //IntPtr data = IntPtr.Zero;

      //resize_main_win.GetProperty(atom, 0, 2, false, atom, out return_type, out actual_format, out nitems, out bytes_return, out data);

      //string z = Marshal.PtrToStringAnsi(data);
      //Console.WriteLine("nitems = {0}, z = {1}", nitems, z);

      //pix = new XPixmap (dpy);

      //if (pix.ReadPixmapFromData (resize_main_win, pix_data)) {
      //  resize_main_win.SetBackgroundPixmap (pix);

      //  Console.WriteLine ("pixmap w = {0} | h = {1}", pix.Width.ToString (), pix.Height.ToString ());
      //}

      resize_main_win.SetBackgroundColor (Color.Gray);

      // Test TransientFor
      //resize_main_win.SetTransientForHint(main_win);
      //Window foo = resize_main_win.GetTransientForHint();
      //Console.WriteLine("resize_main_win = {0} | foo.transient_for = {1} | main_win = {2}", resize_main_win.ID.ToString(), foo.ID.ToString(), main_win.ID.ToString());
      
      XEventMask mask = XEventMask.ButtonPressMask | XEventMask.ButtonReleaseMask;
      
      resize_main_win.SelectInput (mask);
      resize_top_left_win.SelectInput (mask);
      resize_top_right_win.SelectInput (mask);
      resize_bottom_left_win.SelectInput (mask);
      resize_bottom_right_win.SelectInput (mask);
      
      PlaceHandles ();
      
      resize_main_win.MapSubwindows ();
      main_win.MapSubwindows ();
      main_win.Map ();
      
      ev.KeyPressHandlerEvent += new KeyPressHandler (HandleKeyPress);
      ev.MotionNotifyHandlerEvent += new MotionNotifyHandler (HandleMotionNotify);
      ev.ButtonPressHandlerEvent += new ButtonPressHandler (HandleButtonPress);
      ev.ButtonReleaseHandlerEvent += new ButtonReleaseHandler (HandleButtonRelease);
      ev.ExposeHandlerEvent += new ExposeHandler (HandleExpose);
      
      t.Interval = 1000;
      t.Enabled = true;
      t.Elapsed += new ElapsedEventHandler (UpdateClock);
      t.Start ();
      
      ev.Loop ();
    }

    public void DrawClock ()
    {
      resize_main_win.Clear ();
      resize_main_win.DrawString (gc, new Point (10, 40), DateTime.Now.ToString ());
      dpy.Flush ();
    }

    public void UpdateClock (object source, EventArgs e)
    {
      DrawClock ();
    }

    public void PlaceHandles ()
    {
      resize_top_left_win.Move (new Point (-1, -1));
      resize_top_right_win.Move (new Point (resize_win_width - (resize_handle_width + 1), -1));
      resize_bottom_left_win.Move (new Point (-1, resize_win_height - (resize_handle_height + 1)));
      resize_bottom_right_win.Move (new Point (resize_win_width - (resize_handle_width + 1), resize_win_height - (resize_handle_height + 1)));
    }

    public void ResizeTopLeft (int x, int y)
    {
      resize_win_x = attr.x + x;
      resize_win_y = attr.y + y;
      resize_win_width = attr.width - x;
      resize_win_height = attr.height - y;
    }

    public void ResizeBottomLeft (int x, int y)
    {
      resize_win_x = attr.x + x;
      resize_win_width = attr.width - x;
      resize_win_height = attr.height + y;
    }

    public void ResizeTopRight (int x, int y)
    {
      resize_win_y = attr.y + y;
      resize_win_width = attr.width + x;
      resize_win_height = attr.height - y;
    }

    public void ResizeBottomRight (int x, int y)
    {
      resize_win_width = attr.width + x;
      resize_win_height = attr.height + y;
    }

    public void HandleExpose (XExposeEvent e, XWindow window)
    {
      if (e.window == main_win.ID) {
        DrawClock ();
        main_win.DrawString (gc, new Point (10, 20), "Resize / Move test / Clock / Color test... Press 'Q' to quit...");
      }
    }

    public void HandleButtonPress (XButtonEvent e, XWindow window, XWindow root, XWindow subwindow)
    {
      pointer.Grab (e.window, XEventMask.PointerMotionMask | XEventMask.ButtonReleaseMask);
      
      attr = resize_main_win.GetAttributes ();
      
      start = e;
      
      old_resize_x = resize_win_x;
      old_resize_y = resize_win_y;
    }

    public void HandleButtonRelease (XButtonEvent e, XWindow window, XWindow root, XWindow subwindow)
    {
      pointer.Ungrab ();
    }

    public void HandleKeyPress (XKeyEvent e, XWindow window, XWindow root, XWindow subwindow)
    {
      
      if (Convert.ToBoolean (main_win.LookupKeysym (ref e) == XKeySym.XK_q)) {
        Console.WriteLine ("Cleaning up and exiting...");
        
        //pix.Free ();
        
        gc.Dispose ();
        
        bg_color.Dispose ();
        handle_color.Dispose ();
        
        resize_top_left_win.Dispose ();
        resize_top_right_win.Dispose ();
        resize_bottom_left_win.Dispose ();
        resize_bottom_right_win.Dispose ();
        
        resize_main_win.Dispose ();
        
        main_win.Dispose ();
        ev.Dispose ();
        s.Dispose ();
        dpy.Dispose ();
        
        Environment.Exit (0);
      }
    }

    public void HandleMotionNotify (XMotionEvent e, XWindow window, XWindow root, XWindow subwindow)
    {
      int xdiff = e.x_root - start.x_root;
      int ydiff = e.y_root - start.y_root;
      
      if ((e.window == resize_top_left_win.ID) || (e.window == resize_bottom_left_win.ID) || (e.window == resize_top_right_win.ID) || (e.window == resize_bottom_right_win.ID)) {
        if (e.window == resize_top_left_win.ID)
          ResizeTopLeft (xdiff, ydiff); else if (e.window == resize_bottom_left_win.ID)
          ResizeBottomLeft (xdiff, ydiff); else if (e.window == resize_top_right_win.ID)
          ResizeTopRight (xdiff, ydiff); else if (e.window == resize_bottom_right_win.ID)
          ResizeBottomRight (xdiff, ydiff);
        
        if (resize_win_width < min_size || resize_win_height < min_size)
          return;
        
        resize_main_win.MoveResize (new Rectangle (resize_win_x, resize_win_y, resize_win_width, resize_win_height));
        
        DrawClock ();
        
        PlaceHandles ();
      } else if (e.window == resize_main_win.ID) {
        
        int x = old_resize_x + (e.x_root - start.x_root);
        int y = old_resize_y + (e.y_root - start.y_root);
        
        resize_main_win.Move (new Point (x, y));
        
        resize_win_x = x;
        resize_win_y = y;
      }
    }

    static void Main (string[] args)
    {
      new WindowResizeTest ();
    }
  }
}
