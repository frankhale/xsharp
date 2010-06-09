// XSharp - A subset of the Xlib C API wrapped for Mono in C#
//
// This library will eventually be used to port aewm++/mini window managers
// to C#/Mono
// 
// Copyright (C) 2010 
//
// Frank Hale <frankhale@gmail.com> aka majyk
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
using System.Runtime.InteropServices;

namespace XSharp
{
  #region XHandle Class
  public abstract class XHandle : IDisposable
  {
    public IntPtr Handle { get; set; }

    //FIXME: All overrided Dispose() methods should call base.Dispose()
    public virtual void Dispose ()
    {
      Handle = IntPtr.Zero;
    }
  }
  #endregion

  #region XFont Structures
  [StructLayout(LayoutKind.Sequential)]
  public struct XCharStruct
  {
    public short lbearing;
    public short rbearing;
    public short width;
    public short ascent;
    public short descent;
    public ushort attributes;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct XFontProp
  {
    public int name;
    public int card32;
  }

  //FIXME: Need to reevaluate the XFontStruct structure. I don't believe I need to use Explicit layout here.
  [StructLayout(LayoutKind.Explicit, Size = 80)]
  public struct XFontStruct
  {
    [FieldOffset(0)]
    public IntPtr ext_data;
    [FieldOffset(4)]
    public int fid;
    [FieldOffset(8)]
    public int direction;
    [FieldOffset(12)]
    public int min_char_or_byte2;
    [FieldOffset(16)]
    public int max_char_or_byte2;
    [FieldOffset(20)]
    public int min_byte1;
    [FieldOffset(24)]
    public int max_byte1;
    [FieldOffset(28)]
    public int all_chars_exist;
    [FieldOffset(32)]
    public int default_char;
    [FieldOffset(36)]
    public int n_properties;
    [FieldOffset(40)]
    public IntPtr properties;
    [FieldOffset(44)]
    public IntPtr min_bounds;
    [FieldOffset(56)]
    public IntPtr max_bounds;
    [FieldOffset(68)]
    public IntPtr per_char;
    [FieldOffset(72)]
    public int ascent;
    [FieldOffset(76)]
    public int descent;
  }
  #endregion

  #region XFont Class
  public class XFont : XHandle
  {
    private XDisplay display;
    private XFontStruct font;

    public XFont (XDisplay dpy, string name)
    {
      display = dpy;
      Handle = XLoadQueryFont (display.Handle, name);
      
      font = (XFontStruct)Marshal.PtrToStructure (Handle, typeof(XFontStruct));
      
      //Console.WriteLine("Ascent = {0} | Descent = {1}", font.ascent.ToString(), font.descent.ToString());
    }

    public XFontStruct FontStruct {
      get { return font; }
    }

    public override void Dispose ()
    {
      Console.WriteLine ("Freeing Font");
      XFreeFont (display.Handle, Handle);
      base.Dispose ();
    }

    public int Ascent {
      get { return font.ascent; }
    }

    public int Descent {
      get { return font.descent; }
    }

    public int FID {
      get { return font.fid; }
    }

    public XCharStruct TextExtents (string _string)
    {
      int direction_return, font_ascent_return, font_descent_return;
      XCharStruct overall;
      
      IntPtr overall_return = Marshal.AllocHGlobal (Marshal.SizeOf (typeof(XCharStruct)));
      
      XTextExtents (ref font, _string, _string.Length, out direction_return, out font_ascent_return, out font_descent_return, overall_return);
      
      overall = (XCharStruct)Marshal.PtrToStructure (overall_return, typeof(XCharStruct));
      
      Marshal.FreeHGlobal (overall_return);
      
      return overall;
    }

    [DllImport("libX11.so")]
    private static extern IntPtr XLoadQueryFont (IntPtr display, string name);
    [DllImport("libX11.so")]
    private static extern int XFreeFont (IntPtr display, IntPtr font_struct);
    [DllImport("libX11.so")]
    private static extern int XTextExtents (ref XFontStruct font_struct, string _string, int nchars, out int direction_return, out int font_ascent_return, out int font_descent_return, IntPtr overall_return);
  }
  #endregion

  #region XGC Enums and Structures
  [Flags()]
  public enum XGCFunctionMask
  {
    GXclear = 0x0,
    GXand = 0x1,
    GXandReverse = 0x2,
    GXcopy = 0x3,
    GXandInverted = 0x4,
    GXnoop = 0x5,
    GXxor = 0x6,
    GXor = 0x7,
    GXnor = 0x8,
    GXequiv = 0x9,
    GXinvert = 0xa,
    GXorReverse = 0xb,
    GXcopyInverted = 0xc,
    GXorInverted = 0xd,
    GXnand = 0xe,
    GXset = 0xf
  }

  public enum XSubwindowMode
  {
    ClipByChildren = 0,
    IncludeInferiors = 1
  }

  [Flags()]
  public enum XGCValuesMask
  {
    GCFunction = (1 << 0),
    GCPlaneMask = (1 << 1),
    GCForeground = (1 << 2),
    GCBackground = (1 << 3),
    GCLineWidth = (1 << 4),
    GCLineStyle = (1 << 5),
    GCCapStyle = (1 << 6),
    GCJoinStyle = (1 << 7),
    GCFillStyle = (1 << 8),
    GCFillRule = (1 << 9),
    GCTile = (1 << 10),
    GCStipple = (1 << 11),
    GCTileStipXOrigin = (1 << 12),
    GCTileStipYOrigin = (1 << 13),
    GCFont = (1 << 14),
    GCSubwindowMode = (1 << 15),
    GCGraphicsExposures = (1 << 16),
    GCClipXOrigin = (1 << 17),
    GCClipYOrigin = (1 << 18),
    GCClipMask = (1 << 19),
    GCDashOffset = (1 << 20),
    GCDashList = (1 << 21),
    GCArcMode = (1 << 22)
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct XGCValues
  {
    public XGCFunctionMask function;
    public int plane_mask;
    public int foreground;
    public int background;
    public int line_width;
    public int line_style;
    public int cap_style;
    public int join_style;
    public int fill_style;
    public int fill_rule;
    public int arc_mode;
    public int tile;
    public int stipple;
    public int ts_x_origin;
    public int ts_y_origin;
    public int font;
    public XSubwindowMode subwindow_mode;
    public int graphics_exposure;
    public int clip_x_origin;
    public int clip_y_origin;
    public int clip_mask;
    public int dash_offset;
    public char dashes;
  }
  #endregion

  #region XGC Class
  public class XGC : XHandle
  {
    private XDisplay display;
    private bool is_default_gc;

    public XGC (XDisplay dpy)
    {
      display = dpy;
      Handle = XDefaultGC (display.Handle, display.Screen.Number);
      is_default_gc = true;
    }


    public XGC (XDisplay dpy, XGCValuesMask valuemask, XGCValues values)
    {
      InitGC (dpy, new IntPtr (dpy.Root), valuemask, values);
    }

    public XGC (XDisplay dpy, XWindow d, XGCValuesMask valuemask, XGCValues values)
    {
      InitGC (dpy, d.Handle, valuemask, values);
    }

    private void InitGC (XDisplay dpy, IntPtr d, XGCValuesMask valuemask, XGCValues values)
    {
      display = dpy;
      Handle = XCreateGC (display.Handle, d, valuemask, ref values);
      is_default_gc = false;
    }

    public override void Dispose ()
    {
      if (!is_default_gc) {
        XFreeGC (display.Handle, Handle);
        base.Dispose ();
      }
    }

    public int SetForeground (XColor foreground)
    {
      return XSetForeground (display.Handle, Handle, foreground.Pixel);
    }

    public int SetForeground (Color foreground)
    {
      return XSetForeground (display.Handle, Handle, new XColor (display, foreground).Pixel);
    }

    public int SetBackground (XColor background)
    {
      return XSetBackground (display.Handle, Handle, background.Pixel);
    }

    public int SetBackground (Color background)
    {
      return XSetForeground (display.Handle, Handle, new XColor (display, background).Pixel);
    }

    public int CopyArea (XWindow src, XWindow dest, int src_x, int src_y, int width, int height, int dest_x, int dest_y)
    {
      return XCopyArea (display.Handle, src.Handle, dest.Handle, Handle, src_x, src_y, width, height, dest_x, dest_y);
    }

    public int ChangeGC (int valuemask, ref XGCValues values)
    {
      return XChangeGC (display.Handle, Handle, valuemask, ref values);
    }

    [DllImport("libX11.so")]
    private static extern IntPtr XDefaultGC (IntPtr display, int screen_number);
    [DllImport("libX11.so")]
    private static extern IntPtr XCreateGC (IntPtr display, IntPtr d, XGCValuesMask valuemask, ref XGCValues values);
    [DllImport("libX11.so")]
    private static extern int XFreeGC (IntPtr display, IntPtr gc);
    [DllImport("libX11.so")]
    private static extern int XSetForeground (IntPtr display, IntPtr gc, int foreground);
    [DllImport("libX11.so")]
    private static extern int XSetBackground (IntPtr display, IntPtr gc, int background);
    [DllImport("libX11.so")]
    private static extern int XCopyArea (IntPtr display, IntPtr src, IntPtr dest, IntPtr gc, int src_x, int src_y, int width, int height, int dest_x, int dest_y);
    [DllImport("libX11.so")]
    private static extern int XChangeGC (IntPtr display, IntPtr gc, int valuemask, ref XGCValues values);
  }
  #endregion

  #region XAtom Enums
  public enum AtomType
  {
    // XAtom.h
    XA_PRIMARY = 1,
    XA_SECONDARY = 2,
    XA_ARC = 3,
    XA_ATOM = 4,
    XA_BITMAP = 5,
    XA_CARDINAL = 6,
    XA_COLORMAP = 7,
    XA_CURSOR = 8,
    XA_CUT_BUFFER0 = 9,
    XA_CUT_BUFFER1 = 10,
    XA_CUT_BUFFER2 = 11,
    XA_CUT_BUFFER3 = 12,
    XA_CUT_BUFFER4 = 13,
    XA_CUT_BUFFER5 = 14,
    XA_CUT_BUFFER6 = 15,
    XA_CUT_BUFFER7 = 16,
    XA_DRAWABLE = 17,
    XA_FONT = 18,
    XA_INTEGER = 19,
    XA_PIXMAP = 20,
    XA_POINT = 21,
    XA_RECTANGLE = 22,
    XA_RESOURCE_MANAGER = 23,
    XA_RGB_COLOR_MAP = 24,
    XA_RGB_BEST_MAP = 25,
    XA_RGB_BLUE_MAP = 26,
    XA_RGB_DEFAULT_MAP = 27,
    XA_RGB_GRAY_MAP = 28,
    XA_RGB_GREEN_MAP = 29,
    XA_RGB_RED_MAP = 30,
    XA_STRING = 31,
    XA_VISUALID = 32,
    XA_WINDOW = 33,
    XA_WM_COMMAND = 34,
    XA_WM_HINTS = 35,
    XA_WM_CLIENT_MACHINE = 36,
    XA_WM_ICON_NAME = 37,
    XA_WM_ICON_SIZE = 38,
    XA_WM_NAME = 39,
    XA_WM_NORMAL_HINTS = 40,
    XA_WM_SIZE_HINTS = 41,
    XA_WM_ZOOM_HINTS = 42,
    XA_MIN_SPACE = 43,
    XA_NORM_SPACE = 44,
    XA_MAX_SPACE = 45,
    XA_END_SPACE = 46,
    XA_SUPERSCRIPT_X = 47,
    XA_SUPERSCRIPT_Y = 48,
    XA_SUBSCRIPT_X = 49,
    XA_SUBSCRIPT_Y = 50,
    XA_UNDERLINE_POSITION = 51,
    XA_UNDERLINE_THICKNESS = 52,
    XA_STRIKEOUT_ASCENT = 53,
    XA_STRIKEOUT_DESCENT = 54,
    XA_ITALIC_ANGLE = 55,
    XA_X_HEIGHT = 56,
    XA_QUAD_WIDTH = 57,
    XA_WEIGHT = 58,
    XA_POINT_SIZE = 59,
    XA_RESOLUTION = 60,
    XA_COPYRIGHT = 61,
    XA_NOTICE = 62,
    XA_FONT_NAME = 63,
    XA_FAMILY_NAME = 64,
    XA_FULL_NAME = 65,
    XA_CAP_HEIGHT = 66,
    XA_WM_CLASS = 67,
    XA_WM_TRANSIENT_FOR = 68
  }
  #endregion

  #region XAtom Class
  public class XAtom
  {
    XDisplay display;
    int atom;

    public XAtom (XDisplay dpy, string atom_name, bool only_if_exists)
    {
      display = dpy;
      
      atom = XInternAtom (display.Handle, atom_name, only_if_exists);
    }

    public int ID {
      get { return atom; }
    }

    public string Name {
      get { return XGetAtomName (display.Handle, atom); }
    }

    [DllImport("libX11.so")]
    private static extern int XInternAtom (IntPtr display, string atom_name, bool only_if_exists);
    [DllImport("libX11.so")]
    private static extern string XGetAtomName (IntPtr display, int atom);
  }
  #endregion

  #region XAtom Structure
  [StructLayout(LayoutKind.Sequential)]
  public struct _XColor
  {
    public int pixel;
    public ushort red, green, blue;
    public char flags;
    public char pad;
  }
  #endregion

  #region XColor Class
  public class XColor : XHandle
  {
    private XDisplay display;
    private _XColor named_color;

    public XColor (XDisplay dpy, string name)
    {
      IntPtr fg, dummyc;
      
      display = dpy;
      
      fg = Marshal.AllocHGlobal (Marshal.SizeOf (typeof(_XColor)));
      dummyc = Marshal.AllocHGlobal (Marshal.SizeOf (typeof(_XColor)));
      
      XAllocNamedColor (display.Handle, display.Screen.DefaultColormap (), name, fg, dummyc);
      
      Handle = fg;
      
      named_color = (_XColor)Marshal.PtrToStructure (Handle, typeof(_XColor));
      
      Marshal.FreeHGlobal (fg);
      Marshal.FreeHGlobal (dummyc);
    }

    public XColor (XDisplay dpy, Color color) : this(dpy, ColorTranslator.ToHtml (Color.FromArgb (color.R, color.G, color.B)))
    {
    }

    public int Pixel {
      get { return named_color.pixel; }
    }

    public _XColor ColorStruct {
      get { return named_color; }
    }

    [DllImport("libX11.so")]
    private static extern int XAllocNamedColor (IntPtr display, int colormap, string color_name, IntPtr screen_def_return, IntPtr exact_def_return);
    [DllImport("libX11.so")]
    private static extern int XAllocColor (IntPtr display, int colormap, IntPtr screen_int_out);
  }
  #endregion

  #region XDisplay Class
  public class XDisplay : XHandle
  {
    public XScreen Screen { get; private set; }

    public XDisplay (string name)
    {
      Handle = XOpenDisplay (name);
      
      Screen = new XScreen (this);
    }

    public XDisplay () : this(null)
    {
    }

    public override void Dispose ()
    {
      XCloseDisplay (Handle);
      
      base.Dispose ();
    }

    public int Sync (bool discard)
    {
      return XSync (Handle, Convert.ToInt32 (discard));
    }

    public int Root {
      get { return XDefaultRootWindow (Handle); }
    }

    public XScreen DefaultScreen {
      get { return new XScreen (this); }
    }

    public int GrabServer ()
    {
      return XGrabServer (Handle);
    }

    public int UngrabServer ()
    {
      return XUngrabServer (Handle);
    }

    public int Flush ()
    {
      return XFlush (Handle);
    }

    public int XRes ()
    {
      return XWidthOfScreen (Screen.Handle);
    }

    public int YRes ()
    {
      return XHeightOfScreen (Screen.Handle);
    }

    [DllImport("libX11.so")]
    private static extern IntPtr XOpenDisplay (string name);
    [DllImport("libX11.so")]
    private static extern int XCloseDisplay (IntPtr display);
    [DllImport("libX11.so")]
    private static extern int XSync (IntPtr display, int discard);
    [DllImport("libX11.so")]
    private static extern int XDefaultRootWindow (IntPtr display);
    [DllImport("libX11.so")]
    private static extern int XGrabServer (IntPtr display);
    [DllImport("libX11.so")]
    private static extern int XUngrabServer (IntPtr display);
    [DllImport("libX11.so")]
    private static extern int XFlush (IntPtr display);
    [DllImport("libX11.so")]
    private static extern int XWidthOfScreen (IntPtr screen);
    [DllImport("libX11.so")]
    private static extern int XHeightOfScreen (IntPtr screen);
  }
  #endregion

  #region XPixmap Enums and Structures
  //FIXME: XpmAttributes - made first pass at wrapping this but it's possibly still not totally correct!!
  [StructLayout(LayoutKind.Sequential, Size = 140)]
  public struct XpmAttributes
  {
    public XPixmapValueMask valuemask;
    public IntPtr visual;
    public int colormap;
    public int depth;
    public int width;
    public int height;
    public int x_hotspot;
    public int y_hotspot;
    public int cpp;
    public IntPtr pixels;
    public int npixels;
    public IntPtr colorsymbols;
    public int numsymbols;
    public string rgb_fname;
    public int nextensions;
    public IntPtr extensions;
    public int ncolors;
    public IntPtr colorTable;
    /* 3.2 backward compatibility code */
    public string hints_cmt;
    public string colors_cmt;
    public string pixels_cmt;
    /* end 3.2 bc */
    public int mask_pixel;
    /* Color Allocation Directives */
    public bool exactColors;
    public int closeness;
    public int red_closeness;
    public int green_closeness;
    public int blue_closeness;
    public int color_key;
    public IntPtr alloc_pixels;
    public int nalloc_pixels;
    public bool alloc_close_colors;
    public int bitmap_format;
    /* Color functions */
    public IntPtr alloc_color;
    public IntPtr free_colors;
    public IntPtr color_closure;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct XpmColor
  {
    public string _string;
    public string symbolic;
    public string m_color;
    public string g4_color;
    public string g_color;
    public string c_color;
  }

  [Flags()]
  public enum XPixmapValueMask
  {
    XpmVisual = (1 << 0),
    XpmColormap = (1 << 1),
    XpmDepth = (1 << 2),
    XpmSize = (1 << 3),
    /* width & height */    XpmHotspot = (1 << 4),
    /* x_hotspot & y_hotspot */    XpmCharsPerPixel = (1 << 5),
    XpmColorSymbols = (1 << 6),
    XpmRgbFilename = (1 << 7),
    /* 3.2 backward compatibility code */
    XpmInfos = (1 << 8),
    XpmReturnInfos = XpmInfos,
    /* end 3.2 bc */
    XpmReturnPixels = (1 << 9),
    XpmExtensions = (1 << 10),
    XpmReturnExtensions = XpmExtensions,
    XpmExactColors = (1 << 11),
    XpmCloseness = (1 << 12),
    XpmRGBCloseness = (1 << 13),
    XpmColorKey = (1 << 14),
    XpmColorTable = (1 << 15),
    XpmReturnColorTable = XpmColorTable,
    XpmReturnAllocPixels = (1 << 16),
    XpmAllocCloseColors = (1 << 17),
    XpmBitmapFormat = (1 << 18),
    XpmAllocColor = (1 << 19),
    XpmFreeColors = (1 << 20),
    XpmColorClosure = (1 << 21),
    /* XpmInfo value masks bits */
    XpmComments = XpmInfos,
    XpmReturnComments = XpmComments
  }

  public enum XPixmapErrorStatus
  {
    XpmColorError = 1,
    XpmSuccess = 0,
    XpmOpenFailed = -1,
    XpmFileInvalid = -2,
    XpmNoMemory = -3,
    XpmColorFailed = -4
  }
  #endregion

  #region XPixmap Class
  public class XPixmap : XHandle
  {
    private XDisplay display;
    //private int handle;
    private int mask;

    private int width = 0;
    private int height = 0;

    private bool success = false;

    public XPixmap (XDisplay dpy)
    {
      display = dpy;
    }

    public bool ReadFileToPixmap (XWindow d, string filename)
    {
      IntPtr pixmap_return;
      int shapemask_return;
      
      XpmAttributes attr = new XpmAttributes ();
      attr.valuemask = XPixmapValueMask.XpmSize;
      
      if (XpmReadFileToPixmap (display.Handle, d.Handle, filename, out pixmap_return, out shapemask_return, ref attr) == 0) {
        Handle = pixmap_return;
        mask = shapemask_return;
        width = attr.width;
        height = attr.height;
        
        success = true;
        
        return true;
      }
      
      return false;
    }

    public bool ReadPixmapFromData (XWindow d, string[] data)
    {
      IntPtr pixmap_return;
      int shapemask_return;
      
      XpmAttributes attr = new XpmAttributes ();
      attr.valuemask = XPixmapValueMask.XpmSize;
      
      if (XpmCreatePixmapFromData (display.Handle, d.Handle, data, out pixmap_return, out shapemask_return, ref attr) == 0) {
        Handle = pixmap_return;
        mask = shapemask_return;
        width = attr.width;
        height = attr.height;
        
        success = true;
        
        return true;
      }
      
      return false;
    }

    public int Mask {
      get { return mask; }
    }

    public int Width {
      get { return width; }
    }

    public int Height {
      get { return height; }
    }

    public void Free ()
    {
      // Will call XFreePixmap()
      if (success) {
        Console.WriteLine ("Freeing Pixmap");
        XFreePixmap (display.Handle, Handle);
      }
    }

    [DllImport("libX11.so")]
    private static extern int XFreePixmap (IntPtr display, IntPtr pixmap);
    [DllImport("libXpm.so")]
    private static extern int XpmReadFileToPixmap (IntPtr display, IntPtr d, string filename, out IntPtr pixmap_return, out int shapemask_return, ref XpmAttributes attributes);
    [DllImport("libXpm.so")]
    private static extern int XpmCreatePixmapFromData (IntPtr display, IntPtr d, string[] data, out IntPtr pixmap_return, out int shapemask_return, ref XpmAttributes attributes);
  }
  #endregion

  #region XPointer Structure
  public struct XPointerQueryInfo
  {
    public IntPtr root, child;
    public int root_x, root_y;
    public int win_x, win_y;
    public int mask;
  }
  #endregion

  #region XPointer Class
  public class XPointer : XHandle
  {
    private XDisplay display;

    public XPointer (XDisplay dpy)
    {
      display = dpy;
    }

    public int Grab (IntPtr w, XEventMask event_mask)
    {
      return XGrabPointer (display.Handle, w, true, event_mask, XGrabMode.GrabModeAsync, XGrabMode.GrabModeAsync, 0, IntPtr.Zero, 0);
    }

    public int Ungrab ()
    {
      return XUngrabPointer (display.Handle, 0);
    }

    public XPointerQueryInfo Query (XWindow w)
    {
      XPointerQueryInfo pi = new XPointerQueryInfo ();
      
      IntPtr root_return, child_return;
      int root_x_return, root_y_return, win_x_return, win_y_return, mask_return;
      
      XQueryPointer (display.Handle, w.Handle, out root_return, out child_return, out root_x_return, out root_y_return, out win_x_return, out win_y_return, out mask_return);
      
      pi.root = root_return;
      pi.child = child_return;
      pi.root_x = root_x_return;
      pi.root_y = root_y_return;
      pi.win_x = win_x_return;
      pi.win_y = win_y_return;
      pi.mask = mask_return;
      
      return pi;
    }

    public int Warp (XWindow src_w, XWindow dest_w, int src_x, int src_y, int src_width, int src_height, int dest_x, int dest_y)
    {
      return XWarpPointer (display.Handle, src_w.Handle, dest_w.Handle, src_x, src_y, src_width, src_height, dest_x, dest_y);
    }

    [DllImport("libX11.so")]
    private static extern int XUngrabPointer (IntPtr display, int t);
    [DllImport("libX11.so")]
    private static extern int XGrabPointer (IntPtr display, IntPtr grab_window, bool owner_events, XEventMask event_mask, XGrabMode pointer_mode, XGrabMode keyboard_mode, int confine_to, IntPtr cursor, int time);
    [DllImport("libX11.so")]
    private static extern bool XQueryPointer (IntPtr display, IntPtr w, out IntPtr root_return, out IntPtr child_return, out int root_x_return, out int root_y_return, out int win_x_return, out int win_y_return, out int mask_return);
    [DllImport("libX11.so")]
    private static extern int XWarpPointer (IntPtr display, IntPtr src_w, IntPtr dest_w, int src_x, int src_y, int src_width, int src_height, int dest_x, int dest_y);
  }
  #endregion

  #region XScreen Class
  public class XScreen : XHandle
  {
    XDisplay display;

    public XScreen (XDisplay dpy)
    {
      display = dpy;
      
      if (display.Handle == IntPtr.Zero)
        throw new NullReferenceException ("Display pointer is null");
      
      Handle = XDefaultScreenOfDisplay (display.Handle);
    }

    public int Width {
      get { return XWidthOfScreen (Handle); }
    }

    public int Height {
      get { return XHeightOfScreen (Handle); }
    }

    public Rectangle Geometry {
      get { return new Rectangle (0, 0, Width, Height); }
    }

    public int Number {
      get { return XScreenNumberOfScreen (Handle); }
    }

    public int BlackPixel ()
    {
      return XBlackPixel (display.Handle, Number);
    }

    public int WhitePixel ()
    {
      return XWhitePixel (display.Handle, Number);
    }

    public int DefaultColormap ()
    {
      return XDefaultColormapOfScreen (Handle);
    }

    [DllImport("libX11.so")]
    private static extern int XWidthOfScreen (IntPtr screen);
    [DllImport("libX11.so")]
    private static extern int XHeightOfScreen (IntPtr screen);
    [DllImport("libX11.so")]
    private static extern int XScreenNumberOfScreen (IntPtr screen);
    [DllImport("libX11.so")]
    private static extern IntPtr XDefaultScreenOfDisplay (IntPtr display);
    [DllImport("libX11.so")]
    private static extern int XBlackPixel (IntPtr display, int screen_number);
    [DllImport("libX11.so")]
    private static extern int XWhitePixel (IntPtr display, int screen_number);
    [DllImport("libX11.so")]
    private static extern int XDefaultColormapOfScreen (IntPtr screen);
  }
  #endregion

  #region XShape Enums and Structures
  public enum XShapeOperation
  {
    ShapeSet = 0,
    ShapeUnion = 1,
    ShapeIntersect = 2,
    ShapeSubtract = 3,
    ShapeInvert = 4
  }

  public enum XShapeReqType
  {
    X_ShapeQueryVersion = 0,
    X_ShapeRectangles = 1,
    X_ShapeMask = 2,
    X_ShapeCombine = 3,
    X_ShapeOffset = 4,
    X_ShapeQueryExtents = 5,
    X_ShapeSelectInput = 6,
    X_ShapeInputSelected = 7,
    X_ShapeGetRectangles = 8
  }

  public enum XShapeKind
  {
    ShapeBounding = 0,
    ShapeClip = 1,
    ShapeInput = 2
  }

  public enum XShapeEventMask
  {
    ShapeNotifyMask = (1 << 0),
    ShapeNotify = 0
  }

  public struct XShapeVersionInfo
  {
    public int major_version;
    public int minor_version;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct XRectangle
  {
    public short x, y;
    public ushort width, height;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct XBox
  {
    public short x1, x2, y1, y2;
  }

  //FIXME: Need to fix the Region struct and marshall the rects pointer with the correct size.
  [StructLayout(LayoutKind.Sequential)]
  public struct XRegion
  {
    public int size;
    public int numRects;
    public IntPtr rects;
    /*BOX *rects;*/    public XBox extents;
  }
  #endregion

  #region XShape Class
  public class XShape
  {
    private XDisplay display;
    private int shape_event;
    private bool has_shape_ext;

    // Steve can you explain the line of code below? I'm not sure what this means.
    public const int NumberEvent = (int)(XShapeEventMask.ShapeNotify + 1);

    public XShape (XDisplay dpy)
    {
      display = dpy;
      
      has_shape_ext = Query ();
    }

    public int Event {
      get { return shape_event; }
    }

    public bool Query ()
    {
      int error_base;
      
      return XShapeQueryExtension (display.Handle, out shape_event, out error_base);
    }

    public int Type {
      get { return shape_event; }
    }

    public bool HasShape {
      get { return has_shape_ext; }
    }

    public XShapeVersionInfo Version ()
    {
      int major_version;
      int minor_version;
      
      XShapeQueryVersion (display.Handle, out major_version, out minor_version);
      
      XShapeVersionInfo svi = new XShapeVersionInfo ();
      
      svi.major_version = major_version;
      svi.minor_version = minor_version;
      
      return svi;
    }

    [DllImport("libXext.so")]
    private static extern bool XShapeQueryExtension (IntPtr display, out int event_base, out int error_base);
    [DllImport("libXext.so")]
    private static extern int XShapeQueryVersion (IntPtr display, out int major_version, out int minor_version);
    
  }
    /*[DllImport("libXext.so")]
    private static extern void XShapeCombineRegion(IntPtr display, int dest, int dest_kind, int x_off, int y_off, Region region, int op);
    [DllImport("libXext.so")]
    private static extern void XShapeCombineRectangles(IntPtr display, int dest, int dest_kind, int x_off, int y_off, IntPtr rectangles, int n_rects, int op, int ordering);
    [DllImport("libXext.so")]
    private static extern void XShapeCombineMask(IntPtr display, int dest, int dest_kind, int x_off, int y_off, int src, int op);
    [DllImport("libXext.so")]
    private static extern void XShapeCombineShape(IntPtr display, int dest, int dest_kind, int x_off, int y_off, int src, int src_kind, int op);
    [DllImport("libXext.so")]
    private static extern void XShapeOffsetShape(IntPtr display, int dest, int dest_kind, int x_off, int y_off);
    [DllImport("libXext.so")]
    private static extern int XShapeQueryExtents(IntPtr display, int w, bool bounding_shaped, out int x_bounding, out int y_bounding,
      out int w_bounding, out int h_bounding, out bool clip_shaped, out int x_clip, out int y_clip, out int w_clip, out int h_clip);
    [DllImport("libXext.so")]
    private static extern void XShapeSelectInput(IntPtr display, int w, int mask);
    [DllImport("libXext.so")]
    private static extern int XShapeInputSelected(IntPtr display, int w);
    [DllImport("libXext.so")]
    private static extern IntPtr XShapeGetRectangles(IntPtr display, int w, int kind, out int count, out int ordering);*/    
  #endregion

  #region XCursor Enums
  public enum XCursors {
    XC_num_glyphs     = 154,
    XC_X_cursor     = 0,
    XC_arrow    = 2,
    XC_based_arrow_down   = 4,
    XC_based_arrow_up   = 6,
    XC_boat     = 8,
    XC_bogosity     = 10,
    XC_bottom_left_corner   = 12,
    XC_bottom_right_corner  = 14,
    XC_bottom_side    = 16,
    XC_bottom_tee     = 18,
    XC_box_spiral     = 20,
    XC_center_ptr     = 22,
    XC_circle     = 24,
    XC_clock    = 26,
    XC_coffee_mug     = 28,
    XC_cross    = 30,
    XC_cross_reverse  = 32,
    XC_crosshair    = 34,
    XC_diamond_cross  = 36,
    XC_dot      = 38,
    XC_dotbox     = 40,
    XC_double_arrow   = 42,
    XC_draft_large    = 44,
    XC_draft_small    = 46,
    XC_draped_box     = 48,
    XC_exchange     = 50,
    XC_fleur    = 52,
    XC_gobbler    = 54,
    XC_gumby    = 56,
    XC_hand1    = 58,
    XC_hand2    = 60,
    XC_heart    = 62,
    XC_icon     = 64,
    XC_iron_cross     = 66,
    XC_left_ptr     = 68,
    XC_left_side    = 70,
    XC_left_tee     = 72,
    XC_leftbutton     = 74,
    XC_ll_angle     = 76,
    XC_lr_angle     = 78,
    XC_man      = 80,
    XC_middlebutton   = 82,
    XC_mouse    = 84,
    XC_pencil     = 86,
    XC_pirate     = 88,
    XC_plus     = 90,
    XC_question_arrow   = 92,
    XC_right_ptr    = 94,
    XC_right_side     = 96,
    XC_right_tee    = 98,
    XC_rightbutton    = 100,
    XC_rtl_logo     = 102,
    XC_sailboat     = 104,
    XC_sb_down_arrow  = 106,
    XC_sb_h_double_arrow  = 108,
    XC_sb_left_arrow  = 110,
    XC_sb_right_arrow   = 112,
    XC_sb_up_arrow    = 114,
    XC_sb_v_double_arrow  = 116,
    XC_shuttle    = 118,
    XC_sizing     = 120,
    XC_spider     = 122,
    XC_spraycan     = 124,
    XC_star     = 126,
    XC_target     = 128,
    XC_tcross     = 130,
    XC_top_left_arrow   = 132,
    XC_top_left_corner  = 134,
    XC_top_right_corner   = 136,
    XC_top_side     = 138,
    XC_top_tee    = 140,
    XC_trek     = 142,
    XC_ul_angle     = 144,
    XC_umbrella     = 146,
    XC_ur_angle     = 148,
    XC_watch    = 150,
    XC_xterm    = 152
  }
  #endregion

  #region XCursor Class
  public class XCursor : XHandle {
    private XDisplay display;
    
    public XCursor(XDisplay dpy, XCursors shape)
    {
      display = dpy;
      Handle = XCreateFontCursor(display.Handle, Convert.ToInt32(shape));
    }
    
    public override void Dispose() {
      XFreeCursor(display.Handle, Handle);

      base.Dispose();
    } 
    
    public void DefineCursor (XWindow w)
    {
      XDefineCursor (display.Handle, w.Handle, Handle);
      base.Dispose ();
    }      
    
    public void DefineCursorOnRoot ()
    {
      XDefineCursor (display.Handle, new IntPtr(display.Root), Handle); 
    }
    
    [DllImport("libX11.so")]
    private static extern IntPtr XCreateFontCursor(IntPtr display, int shape);
    [DllImport("libX11.so")]
    private static extern int XDefineCursor(IntPtr display, IntPtr w, IntPtr cursor);
    [DllImport("libX11.so")]
    private static extern int XFreeCursor(IntPtr display, IntPtr cursor);
  }
  #endregion

  #region XWindow Enums and Structs
  public enum XMouseButton
  {
    AnyButton = 0,
    Button1 = 1,
    Button2 = 2,
    Button3 = 3,
    Button4 = 4,
    Button5 = 5
  }

  public enum XMapState
  {
    IsUnmapped = 0,
    IsUnviewable = 1,
    IsViewable = 2
  }

  [Flags()]
  public enum XWMHintFlags
  {
    InputHint = (1 << 0),
    StateHint = (1 << 1),
    IconPixmapHint = (1 << 2),
    IconWindowHint = (1 << 3),
    IconPositionHint = (1 << 4),
    IconMaskHint = (1 << 5),
    WindowGroupHint = (1 << 6),
    AllHints = (InputHint | StateHint | IconPixmapHint | IconWindowHint | IconPositionHint | IconMaskHint | WindowGroupHint),
    XUrgencyHint = (1 << 8)
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct XClassHint
  {
    public string res_name;
    public string res_class;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct XWMHints
  {
    public int flags;
    public bool input;
    public int initial_state;
    public int icon_pixmap;
    public IntPtr icon_window;
    public int icon_x, icon_y;
    public int icon_mask;
    public int window_group;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct XAspect
  {
    public int x;
    public int y;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct XSizeHints
  {
    public XSizeHintFlags flags;
    public int x, y;
    public int width;
    public int height;
    public int min_width;
    public int min_height;
    public int max_width;
    public int max_height;
    public int width_inc;
    public int height_inc;
    public XAspect min_aspect;
    public XAspect max_aspect;
    public int base_width;
    public int base_height;
    public int win_gravity;
  }

  [Flags()]
  public enum XSizeHintFlags
  {
    USPosition = (1 << 0),
    USSize = (1 << 1),
    PPosition = (1 << 2),
    PSize = (1 << 3),
    PMinSize = (1 << 4),
    PMaxSize = (1 << 5),
    PResizeInc = (1 << 6),
    PAspect = (1 << 7),
    PBaseSize = (1 << 8),
    PWinGravity = (1 << 9)
  }
  /* obsolete */
  /*PAllHints = (PPosition|PSize|PMinSize|PMaxSize|PResizeInc|PAspect)*/

  public enum XWindowState
  {
    WithdrawnState = 0,
    NormalState = 1,
    IconicState = 3
  }

  public enum PropMode
  {
    PropModeReplace = 0,
    PropModePrepend = 1,
    PropModeAppend = 2
  }

  public enum XKeySym
  {
    XK_BackSpace = 0xff08,
    /* Back space, back char */    XK_Tab = 0xff09,
    XK_Linefeed = 0xff0a,
    /* Linefeed, LF */    XK_Clear = 0xff0b,
    XK_Return = 0xff0d,
    /* Return, enter */    XK_Pause = 0xff13,
    /* Pause, hold */    XK_Scroll_Lock = 0xff14,
    XK_Sys_Req = 0xff15,
    XK_Escape = 0xff1b,
    XK_Delete = 0xffff,
    XK_Home = 0xff50,
    XK_Left = 0xff51,
    /* Move left, left arrow */    XK_Up = 0xff52,
    /* Move up, up arrow */    XK_Right = 0xff53,
    /* Move right, right arrow */    XK_Down = 0xff54,
    /* Move down, down arrow */    XK_Prior = 0xff55,
    /* Prior, previous */    XK_Page_Up = 0xff55,
    XK_Next = 0xff56,
    /* Next */    XK_Page_Down = 0xff56,
    XK_End = 0xff57,
    /* EOL */    XK_Begin = 0xff58,
    /* BOL */    XK_KP_Space = 0xff80,
    /* Space */    XK_KP_Tab = 0xff89,
    XK_KP_Enter = 0xff8d,
    /* Enter */    XK_KP_F1 = 0xff91,
    /* PF1, KP_A, ... */    XK_KP_F2 = 0xff92,
    XK_KP_F3 = 0xff93,
    XK_KP_F4 = 0xff94,
    XK_KP_Home = 0xff95,
    XK_KP_Left = 0xff96,
    XK_KP_Up = 0xff97,
    XK_KP_Right = 0xff98,
    XK_KP_Down = 0xff99,
    XK_KP_Prior = 0xff9a,
    XK_KP_Page_Up = 0xff9a,
    XK_KP_Next = 0xff9b,
    XK_KP_Page_Down = 0xff9b,
    XK_KP_End = 0xff9c,
    XK_KP_Begin = 0xff9d,
    XK_KP_Insert = 0xff9e,
    XK_KP_Delete = 0xff9f,
    XK_KP_Equal = 0xffbd,
    /* Equals */    XK_KP_Multiply = 0xffaa,
    XK_KP_Add = 0xffab,
    XK_KP_Separator = 0xffac,
    /* Separator, often comma */    XK_KP_Subtract = 0xffad,
    XK_KP_Decimal = 0xffae,
    XK_KP_Divide = 0xffaf,
    XK_KP_0 = 0xffb0,
    XK_KP_1 = 0xffb1,
    XK_KP_2 = 0xffb2,
    XK_KP_3 = 0xffb3,
    XK_KP_4 = 0xffb4,
    XK_KP_5 = 0xffb5,
    XK_KP_6 = 0xffb6,
    XK_KP_7 = 0xffb7,
    XK_KP_8 = 0xffb8,
    XK_KP_9 = 0xffb9,
    XK_F1 = 0xffbe,
    XK_F2 = 0xffbf,
    XK_F3 = 0xffc0,
    XK_F4 = 0xffc1,
    XK_F5 = 0xffc2,
    XK_F6 = 0xffc3,
    XK_F7 = 0xffc4,
    XK_F8 = 0xffc5,
    XK_F9 = 0xffc6,
    XK_F10 = 0xffc7,
    XK_F11 = 0xffc8,
    XK_L1 = 0xffc8,
    XK_F12 = 0xffc9,
    XK_L2 = 0xffc9,
    XK_F13 = 0xffca,
    XK_L3 = 0xffca,
    XK_F14 = 0xffcb,
    XK_L4 = 0xffcb,
    XK_F15 = 0xffcc,
    XK_L5 = 0xffcc,
    XK_F16 = 0xffcd,
    XK_L6 = 0xffcd,
    XK_F17 = 0xffce,
    XK_L7 = 0xffce,
    XK_F18 = 0xffcf,
    XK_L8 = 0xffcf,
    XK_F19 = 0xffd0,
    XK_L9 = 0xffd0,
    XK_F20 = 0xffd1,
    XK_L10 = 0xffd1,
    XK_F21 = 0xffd2,
    XK_R1 = 0xffd2,
    XK_F22 = 0xffd3,
    XK_R2 = 0xffd3,
    XK_F23 = 0xffd4,
    XK_R3 = 0xffd4,
    XK_F24 = 0xffd5,
    XK_R4 = 0xffd5,
    XK_F25 = 0xffd6,
    XK_R5 = 0xffd6,
    XK_F26 = 0xffd7,
    XK_R6 = 0xffd7,
    XK_F27 = 0xffd8,
    XK_R7 = 0xffd8,
    XK_F28 = 0xffd9,
    XK_R8 = 0xffd9,
    XK_F29 = 0xffda,
    XK_R9 = 0xffda,
    XK_F30 = 0xffdb,
    XK_R10 = 0xffdb,
    XK_F31 = 0xffdc,
    XK_R11 = 0xffdc,
    XK_F32 = 0xffdd,
    XK_R12 = 0xffdd,
    XK_F33 = 0xffde,
    XK_R13 = 0xffde,
    XK_F34 = 0xffdf,
    XK_R14 = 0xffdf,
    XK_F35 = 0xffe0,
    XK_R15 = 0xffe0,
    XK_Shift_L = 0xffe1,
    /* Left shift */    XK_Shift_R = 0xffe2,
    /* Right shift */    XK_Control_L = 0xffe3,
    /* Left control */    XK_Control_R = 0xffe4,
    /* Right control */    XK_Caps_Lock = 0xffe5,
    /* Caps lock */    XK_Shift_Lock = 0xffe6,
    /* Shift lock */    XK_Meta_L = 0xffe7,
    /* Left meta */    XK_Meta_R = 0xffe8,
    /* Right meta */    XK_Alt_L = 0xffe9,
    /* Left alt */    XK_Alt_R = 0xffea,
    /* Right alt */    XK_Super_L = 0xffeb,
    /* Left super */    XK_Super_R = 0xffec,
    /* Right super */    XK_Hyper_L = 0xffed,
    /* Left hyper */    XK_Hyper_R = 0xffee,
    /* Right hyper */    XK_space = 0x20,
    /* U+0020 SPACE */    XK_exclam = 0x21,
    /* U+0021 EXCLAMATION MARK */    XK_quotedbl = 0x22,
    /* U+0022 QUOTATION MARK */    XK_numbersign = 0x23,
    /* U+0023 NUMBER SIGN */    XK_dollar = 0x24,
    /* U+0024 DOLLAR SIGN */    XK_percent = 0x25,
    /* U+0025 PERCENT SIGN */    XK_ampersand = 0x26,
    /* U+0026 AMPERSAND */    XK_apostrophe = 0x27,
    /* U+0027 APOSTROPHE */    XK_quoteright = 0x27,
    /* deprecated */    XK_parenleft = 0x28,
    /* U+0028 LEFT PARENTHESIS */    XK_parenright = 0x29,
    /* U+0029 RIGHT PARENTHESIS */    XK_asterisk = 0x2a,
    /* U+002A ASTERISK */    XK_plus = 0x2b,
    /* U+002B PLUS SIGN */    XK_comma = 0x2c,
    /* U+002C COMMA */    XK_minus = 0x2d,
    /* U+002D HYPHEN-MINUS */    XK_period = 0x2e,
    /* U+002E FULL STOP */    XK_slash = 0x2f,
    /* U+002F SOLIDUS */    XK_0 = 0x30,
    /* U+0030 DIGIT ZERO */    XK_1 = 0x31,
    /* U+0031 DIGIT ONE */    XK_2 = 0x32,
    /* U+0032 DIGIT TWO */    XK_3 = 0x33,
    /* U+0033 DIGIT THREE */    XK_4 = 0x34,
    /* U+0034 DIGIT FOUR */    XK_5 = 0x35,
    /* U+0035 DIGIT FIVE */    XK_6 = 0x36,
    /* U+0036 DIGIT SIX */    XK_7 = 0x37,
    /* U+0037 DIGIT SEVEN */    XK_8 = 0x38,
    /* U+0038 DIGIT EIGHT */    XK_9 = 0x39,
    /* U+0039 DIGIT NINE */    XK_colon = 0x3a,
    /* U+003A COLON */    XK_semicolon = 0x3b,
    /* U+003B SEMICOLON */    XK_less = 0x3c,
    /* U+003C LESS-THAN SIGN */    XK_equal = 0x3d,
    /* U+003D EQUALS SIGN */    XK_greater = 0x3e,
    /* U+003E GREATER-THAN SIGN */    XK_question = 0x3f,
    /* U+003F QUESTION MARK */    XK_at = 0x40,
    /* U+0040 COMMERCIAL AT */    XK_A = 0x41,
    /* U+0041 LATIN CAPITAL LETTER A */    XK_B = 0x42,
    /* U+0042 LATIN CAPITAL LETTER B */    XK_C = 0x43,
    /* U+0043 LATIN CAPITAL LETTER C */    XK_D = 0x44,
    /* U+0044 LATIN CAPITAL LETTER D */    XK_E = 0x45,
    /* U+0045 LATIN CAPITAL LETTER E */    XK_F = 0x46,
    /* U+0046 LATIN CAPITAL LETTER F */    XK_G = 0x47,
    /* U+0047 LATIN CAPITAL LETTER G */    XK_H = 0x48,
    /* U+0048 LATIN CAPITAL LETTER H */    XK_I = 0x49,
    /* U+0049 LATIN CAPITAL LETTER I */    XK_J = 0x4a,
    /* U+004A LATIN CAPITAL LETTER J */    XK_K = 0x4b,
    /* U+004B LATIN CAPITAL LETTER K */    XK_L = 0x4c,
    /* U+004C LATIN CAPITAL LETTER L */    XK_M = 0x4d,
    /* U+004D LATIN CAPITAL LETTER M */    XK_N = 0x4e,
    /* U+004E LATIN CAPITAL LETTER N */    XK_O = 0x4f,
    /* U+004F LATIN CAPITAL LETTER O */    XK_P = 0x50,
    /* U+0050 LATIN CAPITAL LETTER P */    XK_Q = 0x51,
    /* U+0051 LATIN CAPITAL LETTER Q */    XK_R = 0x52,
    /* U+0052 LATIN CAPITAL LETTER R */    XK_S = 0x53,
    /* U+0053 LATIN CAPITAL LETTER S */    XK_T = 0x54,
    /* U+0054 LATIN CAPITAL LETTER T */    XK_U = 0x55,
    /* U+0055 LATIN CAPITAL LETTER U */    XK_V = 0x56,
    /* U+0056 LATIN CAPITAL LETTER V */    XK_W = 0x57,
    /* U+0057 LATIN CAPITAL LETTER W */    XK_X = 0x58,
    /* U+0058 LATIN CAPITAL LETTER X */    XK_Y = 0x59,
    /* U+0059 LATIN CAPITAL LETTER Y */    XK_Z = 0x5a,
    /* U+005A LATIN CAPITAL LETTER Z */    XK_bracketleft = 0x5b,
    /* U+005B LEFT SQUARE BRACKET */    XK_backslash = 0x5c,
    /* U+005C REVERSE SOLIDUS */    XK_bracketright = 0x5d,
    /* U+005D RIGHT SQUARE BRACKET */    XK_asciicircum = 0x5e,
    /* U+005E CIRCUMFLEX ACCENT */    XK_underscore = 0x5f,
    /* U+005F LOW LINE */    XK_grave = 0x60,
    /* U+0060 GRAVE ACCENT */    XK_quoteleft = 0x60,
    /* deprecated */    XK_a = 0x61,
    /* U+0061 LATIN SMALL LETTER A */    XK_b = 0x62,
    /* U+0062 LATIN SMALL LETTER B */    XK_c = 0x63,
    /* U+0063 LATIN SMALL LETTER C */    XK_d = 0x64,
    /* U+0064 LATIN SMALL LETTER D */    XK_e = 0x65,
    /* U+0065 LATIN SMALL LETTER E */    XK_f = 0x66,
    /* U+0066 LATIN SMALL LETTER F */    XK_g = 0x67,
    /* U+0067 LATIN SMALL LETTER G */    XK_h = 0x68,
    /* U+0068 LATIN SMALL LETTER H */    XK_i = 0x69,
    /* U+0069 LATIN SMALL LETTER I */    XK_j = 0x6a,
    /* U+006A LATIN SMALL LETTER J */    XK_k = 0x6b,
    /* U+006B LATIN SMALL LETTER K */    XK_l = 0x6c,
    /* U+006C LATIN SMALL LETTER L */    XK_m = 0x6d,
    /* U+006D LATIN SMALL LETTER M */    XK_n = 0x6e,
    /* U+006E LATIN SMALL LETTER N */    XK_o = 0x6f,
    /* U+006F LATIN SMALL LETTER O */    XK_p = 0x70,
    /* U+0070 LATIN SMALL LETTER P */    XK_q = 0x71,
    /* U+0071 LATIN SMALL LETTER Q */    XK_r = 0x72,
    /* U+0072 LATIN SMALL LETTER R */    XK_s = 0x73,
    /* U+0073 LATIN SMALL LETTER S */    XK_t = 0x74,
    /* U+0074 LATIN SMALL LETTER T */    XK_u = 0x75,
    /* U+0075 LATIN SMALL LETTER U */    XK_v = 0x76,
    /* U+0076 LATIN SMALL LETTER V */    XK_w = 0x77,
    /* U+0077 LATIN SMALL LETTER W */    XK_x = 0x78,
    /* U+0078 LATIN SMALL LETTER X */    XK_y = 0x79,
    /* U+0079 LATIN SMALL LETTER Y */    XK_z = 0x7a,
    /* U+007A LATIN SMALL LETTER Z */    XK_braceleft = 0x7b,
    /* U+007B LEFT CURLY BRACKET */    XK_bar = 0x7c,
    /* U+007C VERTICAL LINE */    XK_braceright = 0x7d,
    /* U+007D RIGHT CURLY BRACKET */    XK_asciitilde = 0x7e
  }
  /* U+007E TILDE */
  public enum XInputFocus
  {
    RevertToNone = 0,
    RevertToPointerRoot = 1,
    RevertToParent = 2
  }

  public enum XGrabMode
  {
    GrabModeSync = 0,
    GrabModeAsync = 1
  }

  [Flags()]
  public enum XModMask
  {
    ShiftMask = (1 << 0),
    LockMask = (1 << 1),
    ControlMask = (1 << 2),
    Mod1Mask = (1 << 3),
    Mod2Mask = (1 << 4),
    Mod3Mask = (1 << 5),
    Mod4Mask = (1 << 6),
    Mod5Mask = (1 << 7),
    AnyModifier = (1 << 15)
  }

  [Flags()]
  public enum XWindowAttributeFlags
  {
    CWBackPixel = (1 << 1),
    CWBorderPixmap = (1 << 2),
    CWBorderPixel = (1 << 3),
    CWBitGravity = (1 << 4),
    CWWinGravity = (1 << 5),
    CWBackingStore = (1 << 6),
    CWBackingPlanes = (1 << 7),
    CWBackingPixel = (1 << 8),
    CWOverrideRedirect = (1 << 9),
    CWSaveUnder = (1 << 10),
    CWEventMask = (1 << 11),
    CWDontPropagate = (1 << 12),
    CWColormap = (1 << 13),
    CWCursor = (1 << 14)
  }

  [Flags()]
  public enum XConfigureWindowMask
  {
    CWX = (1 << 0),
    CWY = (1 << 1),
    CWWidth = (1 << 2),
    CWHeight = (1 << 3),
    CWBorderWidth = (1 << 4),
    CWSibling = (1 << 5),
    CWStackMode = (1 << 6)
  }

  public enum Gravity
  {
    /* Bit Gravity */
    ForgetGravity = 0,
    NorthWestGravity = 1,
    NorthGravity = 2,
    NorthEastGravity = 3,
    WestGravity = 4,
    CenterGravity = 5,
    EastGravity = 6,
    SouthWestGravity = 7,
    SouthGravity = 8,
    SouthEastGravity = 9,
    StaticGravity = 10
  }

  ///* Window gravity + bit gravity above */
  //UnmapGravity    0 

  [StructLayout(LayoutKind.Sequential)]
  public struct XWindowChanges
  {
    public int x, y;
    public int width, height;
    public int border_width;
    public int sibling;
    public int stack_mode;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct XWindowAttributes
  {
    public int x, y;
    public int width, height;
    public int border_width;
    public int depth;
    public IntPtr visual;
    public IntPtr root;
    public int c_class;
    public int bit_gravity;
    public int win_gravity;
    public int backing_store;
    public long backing_planes;
    public long backing_pixel;
    public bool save_under;
    public IntPtr colormap;
    public bool map_installed;
    public XMapState map_state;
    public long all_event_masks;
    public long your_event_mask;
    public long do_not_propagate_mask;
    public int override_redirect;
    public IntPtr screen;
  }

  //  typedef struct {
  //    Pixmap background_pixmap; /* background or None or ParentRelative */
  //    unsigned long background_pixel; /* background pixel */
  //    Pixmap border_pixmap; /* border of the window */
  //    unsigned long border_pixel; /* border pixel value */
  //    int bit_gravity;    /* one of bit gravity values */
  //    int win_gravity;    /* one of the window gravity values */
  //    int backing_store;    /* NotUseful, WhenMapped, Always */
  //    unsigned long backing_planes;/* planes to be preseved if possible */
  //    unsigned long backing_pixel;/* value to use in restoring planes */
  //    Bool save_under;    /* should bits under be saved? (popups) */
  //    long event_mask;    /* set of events that should be saved */
  //    long do_not_propagate_mask; /* set of events that should not propagate */
  //    Bool override_redirect; /* boolean value for override-redirect */
  //    Colormap colormap;    /* color map to be associated with window */
  //    Cursor cursor;    /* cursor to be displayed (or None) */
  //} XSetWindowAttributes;

  //FIXME: Forgot to set the size of the XSetWindowAttributes struct. Plus I may not even need to define the struct explicity.
  [StructLayout(LayoutKind.Explicit)]
  public struct XSetWindowAttributes
  {
    [FieldOffset(0)]
    public IntPtr background_pixmap;
    [FieldOffset(8)]
    public ulong background_pixel;
    [FieldOffset(16)]
    public IntPtr border_pixmap;
    [FieldOffset(32)]
    public ulong border_pixel;
    [FieldOffset(36)]
    public int bit_gravity;
    [FieldOffset(40)]
    public int win_gravity;
    [FieldOffset(44)]
    public int backing_store;
    [FieldOffset(52)]
    public ulong backing_planes;
    [FieldOffset(60)]
    public ulong backing_pixel;
    [FieldOffset(64)]
    public int save_under;
    [FieldOffset(72)]
    public XEventMask event_mask;
    [FieldOffset(80)]
    public XEventMask do_not_propogate_mask;
    [FieldOffset(84)]
    public int override_redirect;
    [FieldOffset(92)]
    public IntPtr colormap;
    [FieldOffset(100)]
    public IntPtr cursor;
  }
  #endregion

  #region XWindow Class
  public class XWindow : XHandle
  {
    private XDisplay display;
    private bool disposed = false;

    public IntPtr ID {
      get { return Handle; }
    }

    public IntPtr Root {
      get { return XDefaultRootWindow (display.Handle); }
    }

    public string Name {
      get {
        string name = String.Empty;
        
        XFetchName (display.Handle, Handle, out name);
        
        return name;
      }

      set { XStoreName (display.Handle, Handle, value); }
    }

    public int SelectInput (XEventMask mask)
    {
      return XSelectInput (display.Handle, Handle, mask);
    }

    public int DefineCursor (XCursor cursor)
    {
      return XDefineCursor (display.Handle, Handle, cursor.Handle);
    }

    public XWindow (XDisplay dpy)
    {
      display = dpy;
      Handle = Root;
    }

    public XWindow (XDisplay dpy, IntPtr handle)
    {
      display = dpy;
      Handle = handle;
    }

    public XWindow (XDisplay dpy, Rectangle geom)
    {
      display = dpy;
      
      Handle = XCreateSimpleWindow (display.Handle, Root, geom.X, geom.Y, geom.Width, geom.Height, 1, display.Screen.BlackPixel (), display.Screen.WhitePixel ());
    }

    public XWindow (XDisplay dpy, Rectangle geom, int border_width, int border_color, int background_color)
    {
      display = dpy;
      
      Handle = XCreateSimpleWindow (display.Handle, Root, geom.X, geom.Y, geom.Width, geom.Height, border_width, border_color, background_color);
    }

    public XWindow (XDisplay dpy, XWindow parent, Rectangle geom, int border_width, int border_color, int background_color)
    {
      display = dpy;
      
      Handle = XCreateSimpleWindow (display.Handle, parent.Handle, geom.X, geom.Y, geom.Width, geom.Height, border_width, border_color, background_color);
    }

    public override void Dispose ()
    {
      Destroy ();

      base.Dispose();
    }

    public void Destroy ()
    {
      if (!disposed) {
        Console.WriteLine ("Destroying window");
        XDestroyWindow (display.Handle, Handle);
        
        disposed = true;
      }
    }

    public void Map ()
    {
      if (Handle != Root) {
        XMapWindow (display.Handle, Handle);
      }
    }

    public void Unmap ()
    {
      XunmapWindow (display.Handle, Handle);
    }

    public void MapSubwindows ()
    {
      XMapSubwindows (display.Handle, Handle);
    }

    public int Reparent (XWindow p, Point pos)
    {
      return XReparentWindow (display.Handle, Handle, p.Handle, pos.X, pos.Y);
    }

    public int Clear ()
    {
      return XClearWindow (display.Handle, Handle);
    }

    public int DrawString (XGC gc, Point pos, string _string)
    {
      return XDrawString (display.Handle, Handle, gc.Handle, pos.X, pos.Y, _string, _string.Length);
    }

    public int ChangeAttributes (XWindowAttributeFlags mask, XSetWindowAttributes attributes)
    {
      IntPtr attr = Marshal.AllocHGlobal (Marshal.SizeOf (attributes));
      
      Console.WriteLine ("Attributes size = {0}", Marshal.SizeOf (attributes));
//      Console.WriteLine ("Attributes.background_pixmap = {0}", Marshal.SizeOf (attributes.background_pixmap));
//      Console.WriteLine ("Attributes.background_pixel = {0}", Marshal.SizeOf (attributes.background_pixel));
//      Console.WriteLine ("Attributes size = {0}", Marshal.SizeOf (attributes));
//      Console.WriteLine ("Attributes size = {0}", Marshal.SizeOf (attributes));
//      Console.WriteLine ("Attributes size = {0}", Marshal.SizeOf (attributes));
      
      
      Marshal.StructureToPtr (attributes, attr, false);
      
      int result = XChangeWindowAttributes (display.Handle, Handle, mask, attr);
      
      Marshal.FreeHGlobal (attr);
      
      return result;
    }

    public int ChangeAttributesOnRoot (XWindowAttributeFlags mask, XSetWindowAttributes attributes)
    {
      return 0;
      //return XChangeWindowAttributes (display.Handle, Root, mask, ref attributes);
    }

    public XWindowAttributes GetAttributes ()
    {
      XWindowAttributes window_attributes_return;
      
      XGetWindowAttributes (display.Handle, Handle, out window_attributes_return);
      
      return window_attributes_return;
    }

    public List<XWindow> QueryTree ()
    {
      IntPtr root_return;
      IntPtr parent_return;
      IntPtr children_return;
      int nchildren_return;
      
      XQueryTree (display.Handle, Handle, out root_return, out parent_return, out children_return, out nchildren_return);
      
      Console.WriteLine ("Total Windows = {0}", nchildren_return);
      
      IntPtr[] awins = new IntPtr[nchildren_return];
      
      Marshal.Copy (children_return, awins, 0, awins.Length);
      
      List<XWindow> wins = new List<XWindow> ();
      
      foreach (IntPtr wid in awins) {
        XWindow win = new XWindow (display, wid);
        wins.Add (win);
      }
      
      return wins;
    }

    public int Configure (int value_mask, XWindowChanges changes)
    {
      return XConfigureWindow (display.Handle, Handle, value_mask, changes);
    }

    public int GrabButton (int button, XModMask modifiers, IntPtr grab_window, bool owner_events, XEventMask event_mask, XGrabMode pointer_mode, XGrabMode keyboard_mode, int confine_to, IntPtr cursor)
    {
      return XGrabButton (display.Handle, button, modifiers, grab_window, owner_events, event_mask, pointer_mode, keyboard_mode, confine_to, cursor);
    }

    public int GrabButton (int button, XModMask modifiers, XEventMask event_mask)
    {
      return XGrabButton (display.Handle, button, modifiers, Handle, true, event_mask, XGrabMode.GrabModeAsync, XGrabMode.GrabModeAsync, 0, IntPtr.Zero);
    }

    public int UngrabButton (XMouseButton button, XModMask modifiers)
    {
      return XUngrabButton (display.Handle, button, modifiers, Handle);
    }

    public int GrabPointer (XEventMask event_mask)
    {
      return XGrabPointer (display.Handle, Handle, true, event_mask, XGrabMode.GrabModeAsync, XGrabMode.GrabModeAsync, 0, 0, 0);
    }

    public int UngrabPointer ()
    {
      return XUngrabPointer (display.Handle, 0);
    }

    public XPointerQueryInfo QueryPointer ()
    {
      XPointerQueryInfo pi = new XPointerQueryInfo ();
      
      IntPtr root_return, child_return;
      int root_x_return, root_y_return, win_x_return, win_y_return, mask_return;
      
      XQueryPointer (display.Handle, Handle, out root_return, out child_return, out root_x_return, out root_y_return, out win_x_return, out win_y_return, out mask_return);
      
      pi.root = root_return;
      pi.child = child_return;
      pi.root_x = root_x_return;
      pi.root_y = root_y_return;
      pi.win_x = win_x_return;
      pi.win_y = win_y_return;
      pi.mask = mask_return;
      
      return pi;
    }

    public int SetInputFocus (XInputFocus revert_to)
    {
      return XSetInputFocus (display.Handle, Handle, revert_to, 0);
    }

    public int Move (Point pos)
    {
      return XMoveWindow (display.Handle, Handle, pos.X, pos.Y);
    }

    public int Resize (Size size)
    {
      return XResizeWindow (display.Handle, Handle, size.Width, size.Height);
    }

    public int MoveResize (Rectangle geom)
    {
      return XMoveResizeWindow (display.Handle, Handle, geom.X, geom.Y, geom.Width, geom.Height);
    }

    public int MoveResize (int x, int y, int width, int height)
    {
      return XMoveResizeWindow (display.Handle, Handle, x, y, width, height);
    }

    public int Raise ()
    {
      return XRaiseWindow (display.Handle, Handle);
    }

    public int Lower ()
    {
      return XLowerWindow (display.Handle, Handle);
    }

    public int SetBackgroundColor (XColor color)
    {
      return XSetWindowBackground (display.Handle, Handle, color.Pixel);
    }

    public int SetBackgroundColor (Color color)
    {
      return XSetWindowBackground (display.Handle, Handle, new XColor (display, color).Pixel);
    }

    public int GrabKey (XKeySym keysym, XModMask modifiers, bool owner_events, XGrabMode pointer_mode, XGrabMode keyboard_mode)
    {
      return XGrabKey (display.Handle, XKeysymToKeycode (display.Handle, keysym), modifiers, Handle, owner_events, pointer_mode, keyboard_mode);
    }

    public int UngrabKey (XKeySym keysym, XModMask modifiers)
    {
      return XUngrabKey (display.Handle, XKeysymToKeycode (display.Handle, keysym), modifiers, Handle);
    }

    public int KeysymToKeycode (XKeySym keysym)
    {
      return XKeysymToKeycode (display.Handle, keysym);
    }

    public XKeySym KeycodeToKeysym (int keycode)
    {
      return XKeycodeToKeysym (display.Handle, keycode, 0);
    }

    public XKeySym LookupKeysym (ref XKeyEvent key_event)
    {
      return XLookupKeysym (ref key_event, 0);
    }

    public XWindow GetTransientForHint ()
    {
      IntPtr window_return;
      
      XGetTransientForHint (display.Handle, Handle, out window_return);
      
      return new XWindow (display, window_return);
    }

    public int SetTransientForHint (XWindow prop_window)
    {
      return XSetTransientForHint (display.Handle, Handle, prop_window.Handle);
    }

    public int AddToSaveSet ()
    {
      return XAddToSaveSet (display.Handle, Handle);
    }

    public int RemoveFromSaveSet ()
    {
      return XRemoveFromSaveSet (display.Handle, Handle);
    }

    public int KillClient ()
    {
      return XKillClient (display.Handle, Handle);
    }

    public XWMHints GetWMHints ()
    {
      IntPtr ptr = XGetWMHints (display.Handle, Handle);
      XWMHints hints;
      
      hints = (XWMHints)Marshal.PtrToStructure (ptr, typeof(XWMHints));
      
      return hints;
    }

    public void SetWMHints (XWMHints wmhints)
    {
      XSetWMHints (display.Handle, Handle, wmhints);
    }

    public XSizeHints GetNormalHints ()
    {
      IntPtr hints_return;
      XSizeHints hints;
      
      hints_return = XAllocSizeHints ();
      
      XGetNormalHints (display.Handle, Handle, hints_return);
      
      hints = (XSizeHints)Marshal.PtrToStructure (hints_return, typeof(XSizeHints));
      
      XFree (hints_return);
      
      return hints;
    }

    public int SetBackgroundPixmap (XPixmap p)
    {
      return XSetWindowBackgroundPixmap (display.Handle, Handle, p.Handle);
    }

    public int SetWindowBorder (XColor color)
    {
      return XSetWindowBorder (display.Handle, Handle, color.Pixel);
    }

    public int SetWindowBorderWidth (int width)
    {
      return XSetWindowBorderWidth (display.Handle, Handle, width);
    }

    public int ClassHint (out XClassHint class_hint_return)
    {
      return XGetClassHint (display.Handle, Handle, out class_hint_return);
    }

    /*public void SetState(XWindowState state) {
      int[] data = { (int)state, 0 };
      ChangeProperty(
    }*/

//    [DllImport("libX11.so")]
//    private static extern int XGetWindowProperty(IntPtr display, IntPtr w, int property, 
//      int long_offset, int long_length, bool delete, int req_type, 
//      out int actual_type_return, out int actual_format_return, 
//      out int nitems_return, out int bytes_after_return, out IntPtr prop_return);
//    
//    public int GetProperty(XAtom property, int long_offset, int long_length, bool delete, int req_type, out int actual_type_return, 
//              out int actual_format_return, out int nitems_return, out int bytes_after_return, out IntPtr prop_return) 
//    { 
//        return  XGetWindowProperty(display.Handle, id, property.ID, long_offset, long_length, delete, req_type, out actual_type_return, 
//                  out actual_format_return, out nitems_return, out bytes_after_return, out prop_return);
//    }

//    [DllImport("libX11.so")]
//    private static extern int XChangeProperty(IntPtr display, IntPtr w, int property, int type, int format, PropMode mode, IntPtr data, int nelements);
//    
//    public int ChangeProperty(XAtom property, int type, int format, PropMode mode, IntPtr data, int nelements) {
//      return XChangeProperty(display.Handle, id, property.ID, type, format, mode, data, nelements);
//    }  

    [DllImport("libX11.so")]
    private static extern int XQueryTree (IntPtr display, IntPtr w, out IntPtr root_return, out IntPtr parent_return, out IntPtr children_return, out int nchildren_return);
    [DllImport("libX11.so")]
    private static extern IntPtr XDefaultRootWindow (IntPtr display);
    [DllImport("libX11.so")]
    private static extern int XMapWindow (IntPtr display, IntPtr window);
    [DllImport("libX11.so")]
    private static extern int XunmapWindow (IntPtr display, IntPtr window);
    [DllImport("libX11.so")]
    private static extern int XMapSubwindows (IntPtr display, IntPtr window);
    [DllImport("libX11.so")]
    private static extern int XDestroyWindow (IntPtr display, IntPtr window);
    [DllImport("libX11.so")]
    private static extern IntPtr XCreateSimpleWindow (IntPtr display, IntPtr parent, int x, int y, int w, int h, int border_width, int border, int colour);
    [DllImport("libX11.so")]
    private static extern int XStoreName (IntPtr display, IntPtr w, string name);
    [DllImport("libX11.so")]
    private static extern int XFetchName (IntPtr display, IntPtr w, out string window_name_return);
    [DllImport("libX11.so")]
    private static extern int XSelectInput (IntPtr display, IntPtr w, XEventMask mask);
    [DllImport("libX11.so")]
    private static extern int XReparentWindow (IntPtr display, IntPtr w, IntPtr p, int x, int y);
    [DllImport("libX11.so")]
    private static extern int XClearWindow (IntPtr display, IntPtr w);
    [DllImport("libX11.so")]
    private static extern int XDrawString (IntPtr display, IntPtr d, IntPtr gc, int x, int y, string _string, int length);
    [DllImport("libX11.so")]
    private static extern int XChangeWindowAttributes (IntPtr display, IntPtr w, XWindowAttributeFlags mask, IntPtr attributes);
    [DllImport("libX11.so")]
    private static extern int XGrabButton (IntPtr display, int button, XModMask modifiers, IntPtr grab_window, bool owner_events, XEventMask event_mask, XGrabMode pointer_mode, XGrabMode keyboard_mode, int confine_to, IntPtr cursor);
    [DllImport("libX11.so")]
    private static extern int XUngrabButton (IntPtr display, XMouseButton button, XModMask modifiers, IntPtr grab_window);
    [DllImport("libX11.so")]
    private static extern int XConfigureWindow (IntPtr display, IntPtr w, int value_mask, XWindowChanges changes);
    [DllImport("libX11.so")]
    private static extern int XGetWindowAttributes (IntPtr display, IntPtr w, out XWindowAttributes window_attributes_return);
    [DllImport("libX11.so")]
    private static extern int XUngrabPointer (IntPtr display, int t);
    [DllImport("libX11.so")]
    private static extern int XGrabPointer (IntPtr display, IntPtr grab_window, bool owner_events, XEventMask event_mask, XGrabMode pointer_mode, XGrabMode keyboard_mode, int confine_to, int cursor, int time);
    [DllImport("libX11.so")]
    private static extern int XSetInputFocus (IntPtr display, IntPtr focus, XInputFocus revert_to, int time);
    [DllImport("libX11.so")]
    private static extern int XMoveWindow (IntPtr display, IntPtr w, int x, int y);
    [DllImport("libX11.so")]
    private static extern int XResizeWindow (IntPtr display, IntPtr w, int width, int height);
    [DllImport("libX11.so")]
    private static extern int XMoveResizeWindow (IntPtr display, IntPtr w, int x, int y, int width, int height);
    [DllImport("libX11.so")]
    private static extern int XRaiseWindow (IntPtr display, IntPtr w);
    [DllImport("libX11.so")]
    private static extern int XLowerWindow (IntPtr display, IntPtr w);
    [DllImport("libX11.so")]
    private static extern int XSetWindowBackground (IntPtr display, IntPtr w, int background_pixel);
    [DllImport("libX11.so")]
    private static extern int XGrabKey (IntPtr display, int keycode, XModMask modifiers, IntPtr grab_window, bool owner_events, XGrabMode pointer_mode, XGrabMode keyboard_mode);
    [DllImport("libX11.so")]
    private static extern int XUngrabKey (IntPtr display, int keycode, XModMask modifiers, IntPtr grab_window);
    [DllImport("libX11.so")]
    private static extern int XKeysymToKeycode (IntPtr display, XKeySym keysym);
    [DllImport("libX11.so")]
    private static extern XKeySym XKeycodeToKeysym (IntPtr display, int keycode, int index);
    [DllImport("libX11.so")]
    private static extern XKeySym XLookupKeysym (ref XKeyEvent key_event, int index);
    [DllImport("libX11.so")]
    private static extern int XGetTransientForHint (IntPtr display, IntPtr w, out IntPtr window_return);
    [DllImport("libX11.so")]
    private static extern int XSetTransientForHint (IntPtr display, IntPtr w, IntPtr prop_window);
    [DllImport("libX11.so")]
    private static extern int XAddToSaveSet (IntPtr display, IntPtr w);
    [DllImport("libX11.so")]
    private static extern int XRemoveFromSaveSet (IntPtr display, IntPtr w);
    [DllImport("libX11.so")]
    private static extern int XKillClient (IntPtr display, IntPtr resource);
    [DllImport("libX11.so")]
    private static extern int XDeleteProperty (IntPtr display, IntPtr w, XAtom property);
    [DllImport("libX11.so")]
    private static extern IntPtr XGetWMHints (IntPtr display, IntPtr w);
    [DllImport("libX11.so")]
    private static extern int XGetNormalHints (IntPtr display, IntPtr w, IntPtr hints_return);
    [DllImport("libX11.so")]
    private static extern IntPtr XAllocSizeHints ();
    [DllImport("libX11.so")]
    private static extern int XFree (IntPtr data);
    [DllImport("libX11.so")]
    private static extern int XSetWMHints (IntPtr display, IntPtr w, XWMHints wmhints);
    [DllImport("libX11.so")]
    private static extern void XSetWMNormalHints (IntPtr display, IntPtr w, ref XSizeHints hints);
    [DllImport("libX11.so")]
    private static extern int XGetWMProtocols (IntPtr display, IntPtr w, IntPtr protocols_return, out int count_return);
    [DllImport("libX11.so")]
    private static extern int XDrawLine (IntPtr display, IntPtr d, IntPtr gc, int x1, int y1, int x2, int y2);
    [DllImport("libX11.so")]
    private static extern int XDrawRectangle (IntPtr display, IntPtr d, IntPtr gc, int x, int y, int width, int height);
    [DllImport("libX11.so")]
    private static extern int XGetGeometry (IntPtr display, IntPtr d, out IntPtr root_return, out int x_return, out int y_return, out int width_return, out int height_return, out int border_width_return, out int depth_return);
    [DllImport("libX11.so")]
    private static extern int XSetWindowBorder (IntPtr display, IntPtr w, int border_pixel);
    [DllImport("libX11.so")]
    private static extern int XGetClassHint (IntPtr display, IntPtr w, out XClassHint class_hint_return);
    [DllImport("libX11.so")]
    private static extern int XSetWindowBackgroundPixmap (IntPtr display, IntPtr w, IntPtr background_pixmap);
    [DllImport("libX11.so")]
    private static extern int XDefineCursor (IntPtr display, IntPtr w, IntPtr cursor);
    [DllImport("libX11.so")]
    private static extern int XRootWindow (IntPtr display, int screen);
    [DllImport("libX11.so")]
    private static extern bool XQueryPointer (IntPtr display, IntPtr w, out IntPtr root_return, out IntPtr child_return, out int root_x_return, out int root_y_return, out int win_x_return, out int win_y_return, out int mask_return);
    [DllImport("libX11.so")]
    private static extern int XSetWindowBorderWidth (IntPtr display, IntPtr w, int width);
  }
  #endregion

  #region XEvent Enums and Structures
  public enum XEventMode
  {
    AsyncPointer = 0,
    SyncPointer = 1,
    ReplayPointer = 2,
    AsyncKeyboard = 3,
    SyncKeyboard = 4,
    ReplayKeyboard = 5,
    AsyncBoth = 6,
    SyncBoth = 7
  }

  public enum XErrorCode
  {
    Success = 0,
    BadRequest = 1,
    BadValue = 2,
    BadWindow = 3,
    BadPixmap = 4,
    BadAtom = 5,
    BadCursor = 6,
    BadFont = 7,
    BadMatch = 8,
    BadDrawable = 9,
    BadAccess = 10,
    BadAlloc = 11,
    BadColor = 12,
    BadGC = 13,
    BadIDChoice = 14,
    BadName = 15,
    BadLength = 16,
    BadImplementation = 17
  }

  [Flags()]
  public enum XEventMask
  {
    KeyPressMask = (1 << 0),
    KeyReleaseMask = (1 << 1),
    ButtonPressMask = (1 << 2),
    ButtonReleaseMask = (1 << 3),
    EnterWindowMask = (1 << 4),
    LeaveWindowMask = (1 << 5),
    PointerMotionMask = (1 << 6),
    PointerMotionHintMask = (1 << 7),
    Button1MotionMask = (1 << 8),
    Button2MotionMask = (1 << 9),
    Button3MotionMask = (1 << 10),
    Button4MotionMask = (1 << 11),
    Button5MotionMask = (1 << 12),
    ButtonMotionMask = (1 << 13),
    KeymapStateMask = (1 << 14),
    ExposureMask = (1 << 15),
    VisibilityChangeMask = (1 << 16),
    StructureNotifyMask = (1 << 17),
    ResizeRedirectMask = (1 << 18),
    SubstructureNotifyMask = (1 << 19),
    SubstructureRedirectMask = (1 << 20),
    FocusChangeMask = (1 << 21),
    PropertyChangeMask = (1 << 22),
    ColormapChangeMask = (1 << 23)
  }

  [Flags()]
  public enum XEventType
  {
    KeyPress = 2,
    KeyRelease = 3,
    ButtonPress = 4,
    ButtonRelease = 5,
    MotionNotify = 6,
    EnterNotify = 7,
    LeaveNotify = 8,
    FocusIn = 9,
    FocusOut = 10,
    KeymapNotify = 11,
    Expose = 12,
    GraphicsExpose = 13,
    NoExpose = 14,
    VisibilityNotify = 15,
    CreateNotify = 16,
    DestroyNotify = 17,
    UnmapNotify = 18,
    MapNotify = 19,
    MapRequest = 20,
    ReparentNotify = 21,
    ConfigureNotify = 22,
    ConfigureRequest = 23,
    GravityNotify = 24,
    ResizeRequest = 25,
    CirculateNotify = 26,
    CirculateRequest = 27,
    PropertyNotify = 28,
    SelectionClear = 29,
    SelectionRequest = 30,
    SelectionNotify = 31,
    ColormapNotify = 32,
    ClientMessage = 33,
    MappingNotify = 34
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct XAnyEvent
  {
    public XEventType type;
    public long serial;
    public bool send_event;
    public IntPtr display;
    public IntPtr window;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct XKeyEvent
  {
    public XEventType type;
    public long serial;
    public bool send_event;
    public IntPtr display;
    public IntPtr window;
    public IntPtr root;
    public IntPtr subwindow;
    public long time;
    public int x, y;
    public int x_root, y_root;
    public int state;
    public int keycode;
    public bool same_screen;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct XButtonEvent
  {
    public XEventType type;
    public long serial;
    public bool send_event;
    public IntPtr display;
    public IntPtr window;
    public IntPtr root;
    public IntPtr subwindow;
    public long time;
    public int x, y;
    public int x_root, y_root;
    public int state;
    public int button;
    public bool same_screen;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct XMotionEvent
  {
    public XEventType type;
    public long serial;
    public bool send_event;
    public IntPtr display;
    public IntPtr window;
    public IntPtr root;
    public IntPtr subwindow;
    public long time;
    public int x, y;
    public int x_root, y_root;
    public int state;
    public char is_hint;
    public bool same_screen;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct XCrossingEvent
  {
    public XEventType type;
    public long serial;
    public bool send_event;
    public IntPtr display;
    public IntPtr window;
    public IntPtr root;
    public IntPtr subwindow;
    public long time;
    public int x, y;
    public int x_root, y_root;
    public int mode;
    public int detail;
    public int same_screen;
    public int focus;
    public int state;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct XFocusChangeEvent
  {
    public XEventType type;
    public long serial;
    public bool send_event;
    public IntPtr display;
    public IntPtr window;
    public int mode;
    public int detail;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct XExposeEvent
  {
    public XEventType type;
    public long serial;
    public bool send_event;
    public IntPtr display;
    public IntPtr window;
    public int x, y;
    public int width, height;
    public int count;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct XGraphicsExposeEvent
  {
    public XEventType type;
    public long serial;
    public bool send_event;
    public IntPtr display;
    public IntPtr drawable;
    public int x, y;
    public int width, height;
    public int count;
    public int major_code;
    public int minor_code;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct XNoExposeEvent
  {
    public XEventType type;
    public long serial;
    public bool send_event;
    public IntPtr display;
    public IntPtr drawable;
    public int major_code;
    public int minor_code;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct XVisibilityEvent
  {
    public XEventType type;
    public long serial;
    public bool send_event;
    public IntPtr display;
    public IntPtr window;
    public int state;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct XCreateWindowEvent
  {
    public XEventType type;
    public long serial;
    public bool send_event;
    public IntPtr display;
    public IntPtr parent;
    public IntPtr window;
    public int x, y;
    public int width, height;
    public int border_width;
    public int override_redirect;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct XDestroyWindowEvent
  {
    public XEventType type;
    public long serial;
    public bool send_event;
    public IntPtr display;
    public int _event;
    public IntPtr window;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct XUnmapEvent
  {
    public XEventType type;
    public long serial;
    public bool send_event;
    public IntPtr display;
    public int _event;
    public IntPtr window;
    public int from_configure;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct XMapEvent
  {
    public XEventType type;
    public long serial;
    public bool send_event;
    public IntPtr display;
    public int _event;
    public IntPtr window;
    public int override_redirect;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct XMapRequestEvent
  {
    public XEventType type;
    public long serial;
    public bool send_event;
    public IntPtr display;
    public IntPtr parent;
    public IntPtr window;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct XReparentEvent
  {
    public XEventType type;
    public long serial;
    public bool send_event;
    public IntPtr display;
    public int _event;
    public IntPtr window;
    public IntPtr parent;
    public int x, y;
    public int override_redirect;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct XConfigureEvent
  {
    public XEventType type;
    public long serial;
    public bool send_event;
    public IntPtr display;
    public int _event;
    public IntPtr window;
    public int x, y;
    public int width, height;
    public int border_width;
    public int above;
    public int override_redirect;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct XGravityEvent
  {
    public XEventType type;
    public long serial;
    public bool send_event;
    public IntPtr display;
    public int _event;
    public IntPtr window;
    public int x, y;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct XResizeRequestEvent
  {
    public XEventType type;
    public long serial;
    public bool send_event;
    public IntPtr display;
    public IntPtr window;
    public int width, height;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct XConfigureRequestEvent
  {
    public XEventType type;
    public long serial;
    public bool send_event;
    public IntPtr display;
    public IntPtr parent;
    public IntPtr window;
    public int x, y;
    public int width, height;
    public int border_width;
    public int above;
    public int detail;
    public int value_mask;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct XCirculateEvent
  {
    public XEventType type;
    public long serial;
    public bool send_event;
    public IntPtr display;
    public int _event;
    public IntPtr window;
    public int place;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct XCirculateRequestEvent
  {
    public XEventType type;
    public long serial;
    public bool send_event;
    public IntPtr display;
    public IntPtr parent;
    public IntPtr window;
    public int place;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct XPropertyEvent
  {
    public XEventType type;
    public long serial;
    public bool send_event;
    public IntPtr display;
    public IntPtr window;
    public int atom;
    public int time;
    public int state;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct XSelectionClearEvent
  {
    public XEventType type;
    public long serial;
    public bool send_event;
    public IntPtr display;
    public IntPtr window;
    public int selection;
    public int time;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct XSelectionRequestEvent
  {
    public XEventType type;
    public long serial;
    public bool send_event;
    public IntPtr display;
    public IntPtr owner;
    public IntPtr requestor;
    public int selection;
    public IntPtr target;
    public int property;
    public int time;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct XSelectionEvent
  {
    public XEventType type;
    public long serial;
    public bool send_event;
    public IntPtr display;
    public IntPtr requestor;
    public int selection;
    public IntPtr target;
    public int property;
    public int time;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct XColormapEvent
  {
    public XEventType type;
    public long serial;
    public bool send_event;
    public IntPtr display;
    public IntPtr window;
    public int colormap;
    public int _new;
    public int state;
  }

  // FIXME: XClientMessageEvent struct
  [StructLayout(LayoutKind.Sequential)]
  public struct XClientMessageEvent
  {
    public XEventType type;
    public long serial;
    public bool send_event;
    public IntPtr display;
    public IntPtr window;
    public XAtom message_type;
    public int format;
    [StructLayout(LayoutKind.Explicit)]
    public struct data
    {
      [FieldOffset(0)]
      [MarshalAs(UnmanagedType.LPArray, SizeConst = 20)]
      public char[] b;
      [FieldOffset(0)]
      [MarshalAs(UnmanagedType.LPArray, SizeConst = 10)]
      public short[] s;
      [FieldOffset(0)]
      [MarshalAs(UnmanagedType.LPArray, SizeConst = 5)]
      public int[] l;
    }
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct XMappingEvent
  {
    public XEventType type;
    public long serial;
    public bool send_event;
    public IntPtr display;
    public IntPtr window;
    public int request;
    public int first_keycode;
    public int count;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct XErrorEvent
  {
    public XEventType type;
    public IntPtr display;
    public long resourceid;
    public long serial;
    public byte error_code;
    public byte request_code;
    public byte minor_code; 
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct XKeymapEvent
  {
    public XEventType type;
    public long serial;
    public IntPtr display;
    public IntPtr window;
    [MarshalAs(UnmanagedType.LPArray, SizeConst = 20)]
    public char[] key_vector;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct XShapeEvent
  {
    public XEventType type;
    public long serial;
    public bool send_event;
    public IntPtr display;
    public IntPtr window;
    public int kind;
    public int x, y;
    public int width, height;
    public int time;
    public bool shaped;
  }

  [StructLayout(LayoutKind.Sequential, Size = 192)]
  public struct _XEvent
  {
    public XEventType type;
  }

  public delegate void ShapeHandler (XShapeEvent e, XWindow window);
  public delegate void KeyPressHandler (XKeyEvent e, XWindow window, XWindow root, XWindow subwindow);
  public delegate void KeyReleaseHandler (XKeyEvent e, XWindow window, XWindow root, XWindow subwindow);
  public delegate void ButtonPressHandler (XButtonEvent e, XWindow window, XWindow root, XWindow subwindow);
  public delegate void ButtonReleaseHandler (XButtonEvent e, XWindow window, XWindow root, XWindow subwindow);
  public delegate void ExposeHandler (XExposeEvent e, XWindow window);
  public delegate void EnterNotifyHandler (XCrossingEvent e, XWindow window, XWindow root, XWindow subwindow);
  public delegate void LeaveNotifyHandler (XCrossingEvent e, XWindow window, XWindow root, XWindow subwindow);
  public delegate void MotionNotifyHandler (XMotionEvent e, XWindow window, XWindow root, XWindow subwindow);
  public delegate void FocusInHandler (XFocusChangeEvent e, XWindow window);
  public delegate void FocusOutHandler (XFocusChangeEvent e, XWindow window);
  public delegate void KeymapNotifyHandler (XKeymapEvent e, XWindow window);
  public delegate void GraphicsExposeHandler (XGraphicsExposeEvent e, XWindow drawable);
  public delegate void NoExposeHandler (XNoExposeEvent e, XWindow drawable);
  public delegate void VisibilityNotifyHandler (XVisibilityEvent e, XWindow window);
  public delegate void CreateNotifyHandler (XCreateWindowEvent e, XWindow parent, XWindow window);
  public delegate void DestroyNotifyHandler (XDestroyWindowEvent e, XWindow window);
  public delegate void UnmapNotifyHandler (XUnmapEvent e, XWindow window);
  public delegate void MapNotifyHandler (XMapEvent e, XWindow window);
  public delegate void MapRequestHandler (XMapRequestEvent e, XWindow parent, XWindow window);
  public delegate void ReparentNotifyHandler (XReparentEvent e, XWindow parent, XWindow window);
  public delegate void ConfigureNotifyHandler (XConfigureEvent e, XWindow window);
  public delegate void ConfigureRequestHandler (XConfigureRequestEvent e, XWindow window);
  public delegate void GravityNotifyHandler (XGravityEvent e, XWindow window);
  public delegate void ResizeRequestHandler (XResizeRequestEvent e, XWindow window);
  public delegate void CirculateNotifyHandler (XCirculateEvent e, XWindow window);
  public delegate void CirculateRequestHandler (XCirculateRequestEvent e, XWindow parent, XWindow window);
  public delegate void PropertyNotifyHandler (XPropertyEvent e, XWindow window);
  public delegate void SelectionClearHandler (XSelectionClearEvent e, XWindow window);
  public delegate void SelectionRequestHandler (XSelectionRequestEvent e, XWindow owner, XWindow requestor, XWindow target);
  public delegate void SelectionNotifyHandler (XSelectionEvent e, XWindow requestor, XWindow target);
  public delegate void ColormapNotifyHandler (XColormapEvent e, XWindow window);
  public delegate void ClientMessageHandler (XClientMessageEvent e, XWindow window);
  public delegate void MappingNotifyHandler (XMappingEvent e, XWindow window);

  internal delegate int XHandleXError (IntPtr dpy, IntPtr e);
  public delegate int ErrorHandler (XErrorEvent e);
  #endregion

  #region XEvent Class
  public sealed class XEvent : XHandle
  {
    private XDisplay display;
    private _XEvent _xevent;

    public event ShapeHandler ShapeHandlerEvent;
    public event KeyPressHandler KeyPressHandlerEvent;
    public event KeyReleaseHandler KeyReleaseHandlerEvent;
    public event ButtonPressHandler ButtonPressHandlerEvent;
    public event ButtonReleaseHandler ButtonReleaseHandlerEvent;
    public event ExposeHandler ExposeHandlerEvent;
    public event EnterNotifyHandler EnterNotifyHandlerEvent;
    public event LeaveNotifyHandler LeaveNotifyHandlerEvent;
    public event MotionNotifyHandler MotionNotifyHandlerEvent;
    public event FocusInHandler FocusInHandlerEvent;
    public event FocusOutHandler FocusOutHandlerEvent;
    public event KeymapNotifyHandler KeymapNotifyHandlerEvent;
    public event GraphicsExposeHandler GraphicsExposeHandlerEvent;
    public event NoExposeHandler NoExposeHandlerEvent;
    public event VisibilityNotifyHandler VisibilityNotifyHandlerEvent;
    public event CreateNotifyHandler CreateNotifyHandlerEvent;
    public event DestroyNotifyHandler DestroyNotifyHandlerEvent;
    public event UnmapNotifyHandler UnmapNotifyHandlerEvent;
    public event MapNotifyHandler MapNotifyHandlerEvent;
    public event MapRequestHandler MapRequestHandlerEvent;
    public event ReparentNotifyHandler ReparentNotifyHandlerEvent;
    public event ConfigureNotifyHandler ConfigureNotifyHandlerEvent;
    public event ConfigureRequestHandler ConfigureRequestHandlerEvent;
    public event GravityNotifyHandler GravityNotifyHandlerEvent;
    public event ResizeRequestHandler ResizeRequestHandlerEvent;
    public event CirculateNotifyHandler CirculateNotifyHandlerEvent;
    public event CirculateRequestHandler CirculateRequestHandlerEvent;
    public event PropertyNotifyHandler PropertyNotifyHandlerEvent;
    public event SelectionClearHandler SelectionClearHandlerEvent;
    public event SelectionRequestHandler SelectionRequestHandlerEvent;
    public event SelectionNotifyHandler SelectionNotifyHandlerEvent;
    public event ColormapNotifyHandler ColormapNotifyHandlerEvent;
    public event ClientMessageHandler ClientMessageHandlerEvent;
    public event MappingNotifyHandler MappingNotifyHandlerEvent;

    public event ErrorHandler ErrorHandlerEvent;

    public XEvent (XDisplay dpy)
    {
      display = dpy;
      
      Handle = Marshal.AllocHGlobal (Marshal.SizeOf (typeof(_XEvent)));
      
      SetErrorHandler ();
    }

    public override void Dispose ()
    {
      Console.WriteLine ("Disposing of XEvent");
      Marshal.FreeHGlobal (Handle);
      
      base.Dispose ();
    }

    public XEventType Type {
      get { return (XEventType)_xevent.type; }
    }

    public bool CheckTypedEvent (XEventType event_type)
    {
      return XCheckTypedEvent (display.Handle, event_type, Handle);
    }

    public int Next ()
    {
      int status = XNextEvent (display.Handle, Handle);
      
      _xevent = (_XEvent)Marshal.PtrToStructure (Handle, typeof(_XEvent));
      
      return status;
    }

    public XAnyEvent Any {
      get {
        XAnyEvent any;
        
        any = (XAnyEvent)Marshal.PtrToStructure (Handle, typeof(XAnyEvent));
        
        return any;
      }
    }

    public XKeyEvent Key {
      get {
        XKeyEvent key;
        
        key = (XKeyEvent)Marshal.PtrToStructure (Handle, typeof(XKeyEvent));
        
        return key;
      }
    }

    public XButtonEvent Button {
      get {
        XButtonEvent button;
        
        button = (XButtonEvent)Marshal.PtrToStructure (Handle, typeof(XButtonEvent));
        
        return button;
      }
    }

    public XMotionEvent Motion {
      get {
        XMotionEvent motion;
        
        motion = (XMotionEvent)Marshal.PtrToStructure (Handle, typeof(XMotionEvent));
        
        return motion;
      }
    }

    public XCrossingEvent Crossing {
      get {
        XCrossingEvent crossing;
        
        crossing = (XCrossingEvent)Marshal.PtrToStructure (Handle, typeof(XCrossingEvent));
        
        return crossing;
      }
    }

    public XFocusChangeEvent Focus {
      get {
        XFocusChangeEvent focus;
        
        focus = (XFocusChangeEvent)Marshal.PtrToStructure (Handle, typeof(XFocusChangeEvent));
        
        return focus;
      }
    }

    public XExposeEvent Expose {
      get {
        XExposeEvent expose;
        
        expose = (XExposeEvent)Marshal.PtrToStructure (Handle, typeof(XExposeEvent));
        
        return expose;
      }
    }

    public XGraphicsExposeEvent GraphicsExpose {
      get {
        XGraphicsExposeEvent expose;
        
        expose = (XGraphicsExposeEvent)Marshal.PtrToStructure (Handle, typeof(XGraphicsExposeEvent));
        
        return expose;
      }
    }

    public XNoExposeEvent NoExpose {
      get {
        XNoExposeEvent noexpose;
        
        noexpose = (XNoExposeEvent)Marshal.PtrToStructure (Handle, typeof(XNoExposeEvent));
        
        return noexpose;
      }
    }

    public XVisibilityEvent Visibility {
      get {
        XVisibilityEvent visibility;
        
        visibility = (XVisibilityEvent)Marshal.PtrToStructure (Handle, typeof(XVisibilityEvent));
        
        return visibility;
      }
    }

    public XCreateWindowEvent CreateWindow {
      get {
        XCreateWindowEvent create;
        
        create = (XCreateWindowEvent)Marshal.PtrToStructure (Handle, typeof(XCreateWindowEvent));
        
        return create;
      }
    }

    public XDestroyWindowEvent DestroyWindow {
      get {
        XDestroyWindowEvent destroy;
        
        destroy = (XDestroyWindowEvent)Marshal.PtrToStructure (Handle, typeof(XDestroyWindowEvent));
        
        return destroy;
      }
    }

    public XUnmapEvent Unmap {
      get {
        XUnmapEvent unmap;
        
        unmap = (XUnmapEvent)Marshal.PtrToStructure (Handle, typeof(XUnmapEvent));
        
        return unmap;
      }
    }

    public XMapEvent Map {
      get {
        XMapEvent map;
        
        map = (XMapEvent)Marshal.PtrToStructure (Handle, typeof(XMapEvent));
        
        return map;
      }
    }

    public XMapRequestEvent MapRequest {
      get {
        XMapRequestEvent maprequest;
        
        maprequest = (XMapRequestEvent)Marshal.PtrToStructure (Handle, typeof(XMapRequestEvent));
        
        return maprequest;
      }
    }

    public XReparentEvent Reparent {
      get {
        XReparentEvent reparent;
        
        reparent = (XReparentEvent)Marshal.PtrToStructure (Handle, typeof(XReparentEvent));
        
        return reparent;
      }
    }

    public XConfigureEvent Configure {
      get {
        XConfigureEvent configure;
        
        configure = (XConfigureEvent)Marshal.PtrToStructure (Handle, typeof(XConfigureEvent));
        
        return configure;
      }
    }

    public XGravityEvent Gravity {
      get {
        XGravityEvent gravity;
        
        gravity = (XGravityEvent)Marshal.PtrToStructure (Handle, typeof(XGravityEvent));
        
        return gravity;
      }
    }

    public XResizeRequestEvent ResizeRequest {
      get {
        XResizeRequestEvent resizerequest;
        
        resizerequest = (XResizeRequestEvent)Marshal.PtrToStructure (Handle, typeof(XResizeRequestEvent));
        
        return resizerequest;
      }
    }

    public XConfigureRequestEvent ConfigureRequest {
      get {
        XConfigureRequestEvent configurerequest;
        
        configurerequest = (XConfigureRequestEvent)Marshal.PtrToStructure (Handle, typeof(XConfigureRequestEvent));
        
        return configurerequest;
      }
    }

    public XCirculateEvent Circulate {
      get {
        XCirculateEvent circulate;
        
        circulate = (XCirculateEvent)Marshal.PtrToStructure (Handle, typeof(XCirculateEvent));
        
        return circulate;
      }
    }

    public XCirculateRequestEvent CirculateRequest {
      get {
        XCirculateRequestEvent circulaterequest;
        
        circulaterequest = (XCirculateRequestEvent)Marshal.PtrToStructure (Handle, typeof(XCirculateRequestEvent));
        
        return circulaterequest;
      }
    }

    public XPropertyEvent Property {
      get {
        XPropertyEvent property;
        
        property = (XPropertyEvent)Marshal.PtrToStructure (Handle, typeof(XPropertyEvent));
        
        return property;
      }
    }

    public XSelectionClearEvent SelectionClear {
      get {
        XSelectionClearEvent selectionclear;
        
        selectionclear = (XSelectionClearEvent)Marshal.PtrToStructure (Handle, typeof(XSelectionClearEvent));
        
        return selectionclear;
      }
    }

    public XSelectionRequestEvent SelectionRequest {
      get {
        XSelectionRequestEvent selectionrequest;
        
        selectionrequest = (XSelectionRequestEvent)Marshal.PtrToStructure (Handle, typeof(XSelectionRequestEvent));
        
        return selectionrequest;
      }
    }

    public XSelectionEvent Selection {
      get {
        XSelectionEvent selection;
        
        selection = (XSelectionEvent)Marshal.PtrToStructure (Handle, typeof(XSelectionEvent));
        
        return selection;
      }
    }

    public XColormapEvent Colormap {
      get {
        XColormapEvent colormap;
        
        colormap = (XColormapEvent)Marshal.PtrToStructure (Handle, typeof(XColormapEvent));
        
        return colormap;
      }
    }

    public XClientMessageEvent ClientMessage {
      get {
        XClientMessageEvent clientmessage;
        
        clientmessage = (XClientMessageEvent)Marshal.PtrToStructure (Handle, typeof(XClientMessageEvent));
        
        return clientmessage;
      }
    }

    public XMappingEvent Mapping {
      get {
        XMappingEvent mapping;
        
        mapping = (XMappingEvent)Marshal.PtrToStructure (Handle, typeof(XMappingEvent));
        
        return mapping;
      }
    }

    public XErrorEvent Error {
      get {
        XErrorEvent error;
        
        error = (XErrorEvent)Marshal.PtrToStructure (Handle, typeof(XErrorEvent));
        
        return error;
      }
    }

    public XKeymapEvent Keymap {
      get {
        XKeymapEvent keymap;
        
        keymap = (XKeymapEvent)Marshal.PtrToStructure (Handle, typeof(XKeymapEvent));
        
        return keymap;
      }
    }

    public XShapeEvent Shape {
      get {
        XShapeEvent shape;
        
        shape = (XShapeEvent)Marshal.PtrToStructure (Handle, typeof(XShapeEvent));
        
        return shape;
      }
    }

    public int AllowEvents (XEventMode event_mode)
    {
      return XAllowEvents (Handle, event_mode, 0);
    }

//    public int SendEvent (XWindow w, bool propagate, XEventMask event_mask)
//    {
//      return XSendEvent (display.Handle, w.Handle, propagate, event_mask, Handle);
//    }

    public int SendClientMessage (XWindow w, XAtom a, bool propagate, XEventMask event_mask)
    {
      //XEvent e;
      
      //e.type = XEventType.ClientMessage;
      //e.xclient.window = w;
      //e.xclient.message_type = a;
      //e.xclient.format = 32;
      //e.xclient.data.l[0] = x;
      //e.xclient.data.l[1] = CurrentTime;
     
      //FIXME: SendClientMessage
      //XClientMessageEvent cm = new XClientMessageEvent ();
      
      //cm.window = w.Handle;
      //cm.message_type = a;
      //cm.format = 32;
      //cm.data = new XClientMessageEvent.data ();
      
      //return XSendEvent (dpy, w, False, mask, &e); 
        return 0;
    }
    
    public int Pending ()
    {
      return XPending (display.Handle);
    }

    public void Loop ()
    {
      while (true) {
        Next ();
        
        switch (Type) {
        case XEventType.KeyPress:
          if (KeyPressHandlerEvent != null) {
            XKeyEvent key = Key;
            KeyPressHandlerEvent (key, new XWindow (display, key.window), new XWindow (display, key.root), new XWindow (display, key.subwindow));
          }
          break;
        
        case XEventType.KeyRelease:
          if (KeyReleaseHandlerEvent != null) {
            XKeyEvent key = Key;
            KeyReleaseHandlerEvent (key, new XWindow (display, key.window), new XWindow (display, key.root), new XWindow (display, key.subwindow));
          }
          break;
        
        case XEventType.ButtonPress:
          if (ButtonPressHandlerEvent != null) {
            XButtonEvent button = Button;
            ButtonPressHandlerEvent (button, new XWindow (display, button.window), new XWindow (display, button.root), new XWindow (display, button.subwindow));
          }
          break;
        
        case XEventType.ButtonRelease:
          if (ButtonReleaseHandlerEvent != null) {
            XButtonEvent button = Button;
            ButtonReleaseHandlerEvent (button, new XWindow (display, button.window), new XWindow (display, button.root), new XWindow (display, button.subwindow));
          }
          break;
        
        case XEventType.Expose:
          if (ExposeHandlerEvent != null) {
            XExposeEvent expose = Expose;
            ExposeHandlerEvent (expose, new XWindow (display, expose.window));
          }
          break;
        
        case XEventType.EnterNotify:
          if (EnterNotifyHandlerEvent != null) {
            XCrossingEvent crossing = Crossing;
            EnterNotifyHandlerEvent (crossing, new XWindow (display, crossing.window), new XWindow (display, crossing.root), new XWindow (display, crossing.subwindow));
          }
          break;
        
        case XEventType.LeaveNotify:
          if (LeaveNotifyHandlerEvent != null) {
            XCrossingEvent crossing = Crossing;
            LeaveNotifyHandlerEvent (Crossing, new XWindow (display, crossing.window), new XWindow (display, crossing.root), new XWindow (display, crossing.subwindow));
          }
          break;
        
        case XEventType.MotionNotify:
          if (MotionNotifyHandlerEvent != null) {
            XMotionEvent motion = Motion;
            MotionNotifyHandlerEvent (motion, new XWindow (display, motion.window), new XWindow (display, motion.root), new XWindow (display, motion.subwindow));
          }
          break;
        
        case XEventType.FocusIn:
          if (FocusInHandlerEvent != null) {
            XFocusChangeEvent focus = Focus;
            FocusInHandlerEvent (Focus, new XWindow (display, focus.window));
          }
          break;
        
        case XEventType.FocusOut:
          if (FocusOutHandlerEvent != null) {
            XFocusChangeEvent focus = Focus;
            FocusOutHandlerEvent (Focus, new XWindow (display, focus.window));
          }
          break;
        
        case XEventType.KeymapNotify:
          if (KeymapNotifyHandlerEvent != null) {
            XKeymapEvent keymap = Keymap;
            KeymapNotifyHandlerEvent (keymap, new XWindow (display, keymap.window));
          }
          break;
        
        case XEventType.GraphicsExpose:
          if (GraphicsExposeHandlerEvent != null) {
            XGraphicsExposeEvent graphicsexpose = GraphicsExpose;
            GraphicsExposeHandlerEvent (graphicsexpose, new XWindow (display, graphicsexpose.drawable));
          }
          break;
        
        case XEventType.NoExpose:
          if (NoExposeHandlerEvent != null) {
            XNoExposeEvent noexpose = NoExpose;
            NoExposeHandlerEvent (noexpose, new XWindow (display, noexpose.drawable));
          }
          break;
        
        case XEventType.VisibilityNotify:
          if (VisibilityNotifyHandlerEvent != null) {
            XVisibilityEvent visibility = Visibility;
            VisibilityNotifyHandlerEvent (visibility, new XWindow (display, visibility.window));
          }
          break;
        
        case XEventType.CreateNotify:
          if (CreateNotifyHandlerEvent != null) {
            XCreateWindowEvent createwindow = CreateWindow;
            CreateNotifyHandlerEvent (createwindow, new XWindow (display, createwindow.parent), new XWindow (display, createwindow.window));
          }
          break;
        
        case XEventType.DestroyNotify:
          if (DestroyNotifyHandlerEvent != null) {
            XDestroyWindowEvent destroywindow = DestroyWindow;
            DestroyNotifyHandlerEvent (destroywindow, new XWindow (display, destroywindow.window));
          }
          break;
        
        case XEventType.UnmapNotify:
          if (UnmapNotifyHandlerEvent != null) {
            XUnmapEvent unmap = Unmap;
            UnmapNotifyHandlerEvent (unmap, new XWindow (display, unmap.window));
          }
          break;
        
        case XEventType.MapNotify:
          if (MapNotifyHandlerEvent != null) {
            XMapEvent map = Map;
            MapNotifyHandlerEvent (map, new XWindow (display, map.window));
          }
          break;
        
        case XEventType.MapRequest:
          if (MapRequestHandlerEvent != null) {
            XMapRequestEvent maprequest = MapRequest;
            MapRequestHandlerEvent (maprequest, new XWindow (display, maprequest.parent), new XWindow (display, maprequest.window));
          }
          break;
        
        case XEventType.ReparentNotify:
          if (ReparentNotifyHandlerEvent != null) {
            XReparentEvent reparent = Reparent;
            ReparentNotifyHandlerEvent (reparent, new XWindow (display, reparent.parent), new XWindow (display, reparent.window));
          }
          break;
        
        case XEventType.ConfigureNotify:
          if (ConfigureNotifyHandlerEvent != null) {
            XConfigureEvent configure = Configure;
            ConfigureNotifyHandlerEvent (Configure, new XWindow (display, configure.window));
          }
          break;
        
        case XEventType.ConfigureRequest:
          if (ConfigureRequestHandlerEvent != null) {
            XConfigureRequestEvent configure = ConfigureRequest;
            ConfigureRequestHandlerEvent (ConfigureRequest, new XWindow (display, configure.window));
          }
          break;
        
        case XEventType.GravityNotify:
          if (GravityNotifyHandlerEvent != null) {
            XGravityEvent gravity = Gravity;
            GravityNotifyHandlerEvent (gravity, new XWindow (display, gravity.window));
          }
          break;
        
        case XEventType.ResizeRequest:
          if (ResizeRequestHandlerEvent != null) {
            XResizeRequestEvent resizerequest = ResizeRequest;
            ResizeRequestHandlerEvent (resizerequest, new XWindow (display, resizerequest.window));
          }
          break;
        
        case XEventType.CirculateNotify:
          if (CirculateNotifyHandlerEvent != null) {
            XCirculateEvent circulate = Circulate;
            CirculateNotifyHandlerEvent (circulate, new XWindow (display, circulate.window));
          }
          break;
        
        case XEventType.CirculateRequest:
          if (CirculateRequestHandlerEvent != null) {
            XCirculateRequestEvent circulate = CirculateRequest;
            CirculateRequestHandlerEvent (circulate, new XWindow (display, circulate.parent), new XWindow (display, circulate.window));
          }
          break;
        
        case XEventType.PropertyNotify:
          if (PropertyNotifyHandlerEvent != null) {
            XPropertyEvent property = Property;
            PropertyNotifyHandlerEvent (property, new XWindow (display, property.window));
          }
          break;
        
        case XEventType.SelectionClear:
          if (SelectionClearHandlerEvent != null) {
            XSelectionClearEvent selectionclear = SelectionClear;
            SelectionClearHandlerEvent (selectionclear, new XWindow (display, selectionclear.window));
          }
          break;
        
        case XEventType.SelectionRequest:
          if (SelectionRequestHandlerEvent != null) {
            XSelectionRequestEvent selectionrequest = SelectionRequest;
            SelectionRequestHandlerEvent (selectionrequest, new XWindow (display, selectionrequest.owner), new XWindow (display, selectionrequest.requestor), new XWindow (display, selectionrequest.target));
          }
          break;
        
        case XEventType.SelectionNotify:
          if (SelectionNotifyHandlerEvent != null) {
            XSelectionEvent selection = Selection;
            SelectionNotifyHandlerEvent (selection, new XWindow (display, selection.requestor), new XWindow (display, selection.target));
          }
          break;
        
        case XEventType.ColormapNotify:
          if (ColormapNotifyHandlerEvent != null) {
            XColormapEvent colormap = Colormap;
            ColormapNotifyHandlerEvent (colormap, new XWindow (display, colormap.window));
          }
          break;
        
        case XEventType.ClientMessage:
          if (ClientMessageHandlerEvent != null) {
            XClientMessageEvent clientmessage = ClientMessage;
            ClientMessageHandlerEvent (clientmessage, new XWindow (display, clientmessage.window));
          }
          break;
        
        case XEventType.MappingNotify:
          if (MappingNotifyHandlerEvent != null) {
            XMappingEvent mapping = Mapping;
            MappingNotifyHandlerEvent (mapping, new XWindow (display, mapping.window));
          }
          break;
        default:
          
          if (ShapeHandlerEvent != null) {
            XShapeEvent shape = Shape;
            ShapeHandlerEvent (shape, new XWindow (display, shape.window));
          }
          break;
        }
      }
    }

    [DllImport("libX11.so")]
    private static extern int XNextEvent (IntPtr display, IntPtr handle);
    [DllImport("libX11.so")]
    private static extern bool XCheckTypedEvent (IntPtr display, XEventType event_type, IntPtr event_return);
    [DllImport("libX11.so")]
    private static extern int XAllowEvents (IntPtr display, XEventMode event_mode, int time);
    [DllImport("libX11.so")]
    private static extern int XSendEvent (IntPtr display, IntPtr w, bool propagate, XEventMask event_mask, IntPtr event_send);
    [DllImport("libX11.so")]
    private static extern int XPending (IntPtr display);
    [DllImport("libX11.so")]
    private static extern int XSetErrorHandler (XHandleXError err);

    public void SetErrorHandler ()
    {
      XSetErrorHandler (HandleError);
    }

    private int HandleError (IntPtr d, IntPtr e)
    {
      XErrorEvent err;
      
      err = (XErrorEvent)Marshal.PtrToStructure (e, typeof(XErrorEvent));
      
      Console.WriteLine ("Size of XErrorEvent = {0}", Marshal.SizeOf (err));
      Console.WriteLine ("Size of IntPtr = {0}", Marshal.SizeOf(err.display));
      
      return ErrorHandlerEvent (err);
    }
  }
  #endregion
}