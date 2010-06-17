// __PROTOTYPE_PHASE__
//
// Mini# - A port of a C based minimal window manager for X11
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
using System.Collections.Generic;
using System.Drawing;
using XSharp;

namespace mini
{
  #region Enum's
  enum JUSTIFY
  {
    LEFT_JUSTIFY,
    CENTER_JUSTIFY,
    RIGHT_JUSTIFY
  }

  enum FOCUS
  {
    FOCUS_FOLLOW,
    FOCUS_SLOPPY,
    FOCUS_CLICK
  }

  enum GRAVITY
  {
    APPLY_GRAVITY = 1,
    REMOVE_GRAVITY = -1
  }

  enum RESIZE_MODE
  {
    PIXELS = 0,
    INCREMENTS = 1
  }

  enum WINDOW_PLACEMENT
  {
    MOUSE,
    RANDOM
  }
  #endregion

  public class Mini
  {
    #region DEFAULT SETTINGS
    private static string VERSION_STRING = "mini# window manager (andromeda) | 16 june 2010 | http://github.com/frankhale | frank hale <frankhale@gmail.com>";
    private static string DEFAULT_FONT = "Fixed";
    private static string DEFAULT_FOREGROUND_COLOR = "#ffffff";
    private static string DEFAULT_BACKGROUND_COLOR = "#555555";
    private static string DEFAULT_FOCUS_COLOR = "#dddddd";
    private static string DEFAULT_BORDER_COLOR = "#000000";
    private static string FOCUSED_BORDER_COLOR = "#000000";
    private static string UNFOCUSED_BORDER_COLOR = "#888888";
    private static string FOCUSED_WINDOW_TITLE_COLOR = "#FFFFFF";
    private static string DEFAULT_CMD = "xterm -ls -sb -bg black -fg white";
    private static int DEFAULT_BORDER_WIDTH = 1;
    private static int SPACE = 3;
    private static int MINSIZE = 15;
    private static bool EDGE_SNAP = true;
    private static int SNAP = 5;
    private static JUSTIFY TEXT_JUSTIFY = JUSTIFY.RIGHT_JUSTIFY;
    private static bool WIRE_MOVE = false;
    private static FOCUS DEFAULT_FOCUS_MODEL = FOCUS.FOCUS_CLICK;
    private static WINDOW_PLACEMENT DEFAULT_WINDOW_PLACEMENT = WINDOW_PLACEMENT.MOUSE;
    private static int TRANSIENT_WINDOW_HEIGHT = 8;
    private static int ALT_KEY_COUNT = 2;
    #endregion

    #region Client Struct
    private struct Client
    {
      public XWindow window;
      public XWindow frame;
      public XWindow title;
      public XWindow trans;

      public bool has_focus;
      public bool has_title;
      public bool has_border;
      public bool is_being_dragged;
      public bool is_being_resized;
      public bool do_drawoutline_once;

      public bool is_shaded;
      public bool is_maximized;
      public bool is_visible;
      public bool has_been_shaped;

      public bool button_pressed;
      public TimeSpan last_button1_time;
      public int ignore_unmap;

      public XSizeHints size;
      public int border_width;
      public int x;
      public int y;
      public int width;
      public int height;

      public int old_x;
      public int old_y;
      public int old_width;
      public int old_height;
      public int pointer_x;
      public int pointer_y;
      public int old_cx;
      public int old_cy;

      public string name;
      public XCharStruct overall;
      public int direction;
      public int ascent;
      public int descent;
      public int text_width;
      public int text_justify;
    }
    #endregion

    #region PRIVATE MEMBER FIELDS
    private XEvent ev;

    private string command_line;
    private List<Client> client_list;
    private List<XWindow> client_window_list;

    private Client? focused_client;
    private XFont font;

    private XGC invert_gc;
    private XGC string_gc;
    private XGC border_gc;
    private XGC unfocused_gc;
    private XGC focused_title_gc;

    private XColor fg;
    private XColor bg;
    private XColor bd;
    private XColor fc;
    private XColor focused_border;
    private XColor unfocused_border;

    private XCursor move_curs;
    private XCursor arrow_curs;

    private XDisplay dpy;
    private XWindow root;
    private XWindow _button_proxy_win;

    private bool random_window_placement;

    private FOCUS focus_model;

    //private int shape;
    //private int shape_event;

    private XKeySym[] alt_keys;

    private XAtom atom_wm_state;
    private XAtom atom_wm_change_state;
    private XAtom atom_wm_protos;
    private XAtom atom_wm_delete;
    private XAtom atom_wm_takefocus;
    #endregion

    public Mini (string[] args)
    {
      if (args.Length > 0) {
        if (args[0].Equals ("--version")) {
          Console.WriteLine (VERSION_STRING);
          Environment.Exit (0);
        }
      }
      
      XGCValues gv;
      XSetWindowAttributes sattr;
      focused_client = null;
      focus_model = DEFAULT_FOCUS_MODEL;
      
      //  for (int i = 0; i < argc; i++)
      //    command_line = command_line + argv[i] + " ";
      
      try {
        dpy = new XDisplay (":0");
        
        try {
          font = new XFont (dpy, DEFAULT_FONT);
        } catch {
          font = new XFont (dpy, "Fixed");
        }
      } catch (Exception e) {
        Console.WriteLine ("{0} check your DISPLAY variable.", e.Message);
        Environment.Exit (-1);
      }
      
      XEvent ev = new XEvent (dpy);
      ev.ErrorHandlerEvent += new ErrorHandler (ErrorHandler);
      
      // SET UP ATOMS
      atom_wm_state = new XAtom (dpy, "WM_STATE", false);
      atom_wm_change_state = new XAtom (dpy, "WM_CHANGE_STATE", false);
      atom_wm_protos = new XAtom (dpy, "WM_PROTOCOLS", false);
      atom_wm_delete = new XAtom (dpy, "WM_DELETE_WINDOW", false);
      atom_wm_takefocus = new XAtom (dpy, "WM_TAKE_FOCUS", false);
      
      XSetWindowAttributes pattr = new XSetWindowAttributes ();
      pattr.override_redirect = true;
      _button_proxy_win = new XWindow (dpy, new Rectangle (-80, -80, 24, 24));
      _button_proxy_win.ChangeAttributes (XWindowAttributeFlags.CWOverrideRedirect, pattr);
      
      // SETUP COLORS USED FOR WINDOW TITLE BARS and WINDOW BORDERS
      fg = new XColor (dpy, DEFAULT_FOREGROUND_COLOR);
      bg = new XColor (dpy, DEFAULT_BACKGROUND_COLOR);
      bd = new XColor (dpy, DEFAULT_BORDER_COLOR);
      fc = new XColor (dpy, DEFAULT_FOCUS_COLOR);
      focused_border = new XColor (dpy, FOCUSED_BORDER_COLOR);
      unfocused_border = new XColor (dpy, UNFOCUSED_BORDER_COLOR);
      
      //shape = XShapeQueryExtension(dpy, &shape_event, &dummy);
      
      move_curs = new XCursor (dpy, XCursors.XC_fleur);
      arrow_curs = new XCursor (dpy, XCursors.XC_left_ptr);
      
      root.DefineCursor (arrow_curs);
      
      gv.function = XGCFunctionMask.GXcopy;
      gv.foreground = fg.Pixel;
      gv.font = font.FID;
      string_gc = new XGC (dpy, root, XGCValuesMask.GCFunction | XGCValuesMask.GCForeground | XGCValuesMask.GCFont, gv);
      
      gv.foreground = unfocused_border.Pixel;
      unfocused_gc = new XGC (dpy, root, XGCValuesMask.GCForeground | XGCValuesMask.GCFont, gv);
      
      gv.foreground = fg.Pixel;
      focused_title_gc = new XGC (dpy, root, XGCValuesMask.GCForeground | XGCValuesMask.GCFont, gv);
      
      gv.foreground = bd.Pixel;
      gv.line_width = DEFAULT_BORDER_WIDTH;
      border_gc = new XGC (dpy, root, XGCValuesMask.GCFunction | XGCValuesMask.GCForeground | XGCValuesMask.GCLineWidth, gv);
      
      gv.foreground = fg.Pixel;
      gv.function = XGCFunctionMask.GXinvert;
      gv.subwindow_mode = XSubwindowMode.IncludeInferiors;
      invert_gc = new XGC (dpy, root, XGCValuesMask.GCForeground | XGCValuesMask.GCFunction | XGCValuesMask.GCSubwindowMode | XGCValuesMask.GCLineWidth | XGCValuesMask.GCFont, gv);
      
      sattr.event_mask = XEventMask.SubstructureRedirectMask | XEventMask.SubstructureNotifyMask | XEventMask.ButtonPressMask | XEventMask.ButtonReleaseMask | XEventMask.FocusChangeMask | XEventMask.EnterWindowMask | XEventMask.LeaveWindowMask | XEventMask.PropertyChangeMask | XEventMask.ButtonMotionMask;
      
      root.ChangeAttributes (XWindowAttributeFlags.CWEventMask, sattr);
      
      queryWindowTree ();
      
      ev.KeyPressHandlerEvent += new KeyPressHandler (handleKeyPressEvent);
      ev.ButtonPressHandlerEvent += new ButtonPressHandler (handleButtonPressEvent);
      ev.ButtonReleaseHandlerEvent += new ButtonReleaseHandler (handleButtonReleaseEvent);
      ev.ConfigureRequestHandlerEvent += new ConfigureRequestHandler (handleConfigureRequestEvent);
      ev.MotionNotifyHandlerEvent += new MotionNotifyHandler (handleMotionNotifyEvent);
      ev.MapRequestHandlerEvent += new MapRequestHandler (handleMapRequestEvent);
      ev.UnmapNotifyHandlerEvent += new UnmapNotifyHandler (handleUnmapNotifyEvent);
      ev.DestroyNotifyHandlerEvent += new DestroyNotifyHandler (handleDestroyNotifyEvent);
      ev.EnterNotifyHandlerEvent += new EnterNotifyHandler (handleEnterNotifyEvent);
      ev.FocusInHandlerEvent += new FocusInHandler (handleFocusInEvent);
      ev.FocusOutHandlerEvent += new FocusOutHandler (handleFocusOutEvent);
      ev.PropertyNotifyHandlerEvent += new PropertyNotifyHandler (handlePropertyNotifyEvent);
      ev.ExposeHandlerEvent += new ExposeHandler (handleExposeEvent);
      ev.ShapeHandlerEvent += new ShapeHandler (handleShapeEvent);
    }

    public int ErrorHandler (XErrorEvent e)
    {
//      if ((e.error_code == XErrorCode.BadAccess) &&
//           (e.resourceid == root.Handle))
//      {
//        Console.WriteLine("The root window unavailable!");
//        Environment.Exit(-1);
//      }

      return 0;
    }

    void handleKeyPressEvent (XKeyEvent e, XWindow window, XWindow root, XWindow subwindow)
    {
      XKeySym ks = window.KeycodeToKeysym(e.keycode);

      if (ks==  XKeySym.NoSymbol)
          return;

      switch(ks)
      {
        case XKeySym.XK_Delete:
          Console.WriteLine("Window manager is restarting...");
          restart();
          break;

        case XKeySym.XK_End:
          Console.WriteLine("Window manager is quitting.");
          quitNicely();
          break;
       }
    }

    void handleButtonPressEvent (XButtonEvent e, XWindow window, XWindow root, XWindow subwindow)
    {
    }
    void handleButtonReleaseEvent (XButtonEvent e, XWindow window, XWindow root, XWindow subwindow)
    {
    }
    void handleConfigureRequestEvent (XConfigureRequestEvent e, XWindow window)
    {
    }
    void handleMotionNotifyEvent (XMotionEvent e, XWindow window, XWindow root, XWindow subwindow)
    {
    }
    void handleMapRequestEvent (XMapRequestEvent e, XWindow parent, XWindow window)
    {
    }
    void handleUnmapNotifyEvent (XUnmapEvent e, XWindow window)
    {
    }
    void handleDestroyNotifyEvent (XDestroyWindowEvent e, XWindow window)
    {
    }
    void handleEnterNotifyEvent (XCrossingEvent e, XWindow window, XWindow root, XWindow subwindow)
    {
    }
    void handleFocusInEvent (XFocusChangeEvent e, XWindow window)
    {
    }
    void handleFocusOutEvent (XFocusChangeEvent e, XWindow window)
    {
    }
    void handlePropertyNotifyEvent (XPropertyEvent e, XWindow window)
    {
    }
    void handleExposeEvent (XExposeEvent e, XWindow window)
    {
    }
    void handleShapeEvent (XShapeEvent e, XWindow window)
    {
    }

    void handleClientButtonEvent (XButtonEvent e, Client c)
    {
    }
    void handleClientConfigureRequest (XConfigureRequestEvent e, Client c)
    {
    }
    void handleClientMapRequest (XMapRequestEvent e, Client c)
    {
    }
    void handleClientUnmapEvent (XUnmapEvent e, Client c)
    {
    }
    void handleClientDestroyEvent (XDestroyWindowEvent e, Client c)
    {
    }
    void handleClientPropertyChange (XPropertyEvent e, Client c)
    {
    }
    void handleClientEnterEvent (XCrossingEvent e, Client c)
    {
    }
    void handleClientExposeEvent (XExposeEvent e, Client c)
    {
    }
    void handleClientFocusInEvent (XFocusChangeEvent e, Client c)
    {
    }
    void handleClientMotionNotifyEvent (XMotionEvent e, Client c)
    {
    }
    void handleClientShapeChange (XShapeEvent e, Client c)
    {
    }

    private void cleanup ()
    {
    }

    private void queryWindowTree ()
    {
      XWindowAttributes attr;
      
      List<XWindow> wins = root.QueryTree ();
      
      foreach (XWindow w in wins) {
        attr = w.GetAttributes ();
        
        if (attr.override_redirect && (attr.map_state == XMapState.IsViewable))
          addClient (w);
      }
      
      _button_proxy_win.Map ();
      _button_proxy_win.SetInputFocus (XInputFocus.RevertToNone);
      grabKeys (_button_proxy_win);
    }

    private void quitNicely ()
    {
    }

    private void restart ()
    {
    }

    void addClient (XWindow w)
    {
      XWindowAttributes attr;
      XWMHints hints;
      
      client_window_list.Add (w);
      
      Client c = new Client ();
      client_list.Add (c);
      dpy.GrabServer ();
      
      initializeClient (c);
      
      c.window = w;
      
      queryClientName (c);
      
      c.trans = w.GetTransientForHint ();
      
      attr = c.window.GetAttributes ();
      
      c.x = attr.x;
      c.y = attr.y;
      c.width = attr.width;
      c.height = attr.height;
      c.border_width = attr.border_width;
      c.size = w.GetNormalHints ();
      
      c.old_x = c.x;
      c.old_y = c.y;
      c.old_width = c.width;
      c.old_height = c.height;
      
      if (attr.map_state == XMapState.IsViewable) {
        c.ignore_unmap++;
        
        initClientPosition (c);
        
        hints = w.GetWMHints ();
        
        if ((hints.flags & XWMHintFlags.StateHint) == XWMHintFlags.StateHint)
          setWMState (c.window, hints.initial_state);
        else
          setWMState (c.window, XWindowState.NormalState);
      }
      
      gravitateClient (c, GRAVITY.APPLY_GRAVITY);
      
      reparentClient (c);
      
      unhideClient (c);
      
      if (focus_model == FOCUS.FOCUS_CLICK)
        c.window.SetInputFocus (XInputFocus.RevertToNone);
      
      dpy.Sync (false);
      dpy.UngrabServer ();
    }

    void removeClient (Client c)
    {
      dpy.GrabServer ();
      
      if (c.trans != null)
        c.window.Unmap ();
      
      c.frame.UngrabButton (XMouseButton.AnyButton, XModMask.AnyModifier);
      
      gravitateClient (c, GRAVITY.REMOVE_GRAVITY);
      
      c.window.Reparent (root, new Point (c.x, c.y));
      
      c.title.Destroy ();
      c.frame.Destroy ();
      
      dpy.Sync (false);
      dpy.UngrabServer ();
      
      client_window_list.Remove (c.window);
      client_list.Remove (c);
    }

    Client? findClient (XWindow w)
    {
      foreach (Client c in client_list) {
        if (w == c.title || w == c.frame || w == c.window)
          return (c);
      }
      
      return null;
    }

    void setClientFocus (Client c, bool focus)
    {
    }

    void hideClient (Client c)
    {
    }

    void unhideClient (Client c)
    {
    }

    void shadeClient (Client c)
    {
    }

    void maximizeClient (Client c)
    {
    }

    void initializeClient (Client c)
    {
    }

    void redrawClient (Client c)
    {
    }

    void drawClientOutline (Client c)
    {
    }

    int getClientIncsize (Client c, out int x_ret, out int y_ret, int mode)
    {
      x_ret = 0;
      y_ret = 0;
      
      return 0;
    }

    void initClientPosition (Client c)
    {
    }

    void reparentClient (Client c)
    {
    }

    int getClientTitleHeight (Client c)
    {
      return 0;
      
    }

    void sendClientConfig (Client c)
    {
    }

    void gravitateClient (Client c, GRAVITY multiplier)
    {
    }

    void setClientShape (Client c)
    {
    }

    void queryClientName (Client c)
    {
    }

    void forkExec (string cmd)
    {
    }

    void unfocusAnyStrayClients ()
    {
    }

    void focusPreviousWindowInStackingOrder ()
    {
    }

    long getWMState (XWindow window)
    {
      return 0;
    }

    void setWMState (XWindow window, XWindowState state)
    {
    }

    void sendWMDelete (XWindow window)
    {
    }

    int sendXMessage (XWindow w, XAtom a, long mask, long x)
    {
      return 0;
    }

    void setFocusModel (int new_fm)
    {
    }

    void grabKeys (XWindow w)
    {
      foreach (XKeySym key in alt_keys)
        w.GrabKey (key, (XModMask.Mod1Mask | XModMask.ControlMask), true, XGrabMode.GrabModeAsync, XGrabMode.GrabModeAsync);
    }

    void ungrabKeys (XWindow w)
    {
      foreach (XKeySym key in alt_keys)
        w.UngrabKey (key, (XModMask.Mod1Mask | XModMask.ControlMask));
    }

    void getMousePosition (out int x, out int y)
    {
      x = 0;
      y = 0;
    }
  }

  class MainClass
  {
    public static void Main (string[] args)
    {
      new Mini (args);
    }
  }
}

