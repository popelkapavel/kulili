using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
//using System.Media;

namespace kulili
{
  public partial class Form1 : Form {
    static int anim_max=7,timer_interval=30;
    static string imagepath=@".";
    static Image img_wall,img_full,img_empty,img_me,img_me2,img_he,img_he2;
//    static SoundPlayer move_sound,full_sound;
    static int ww=16,hh=16,scale=1,sx=0,sy=0;
    static My my=new My(),my2=new My();
    static He he=new He(),he2=new He();
    static bool mous,cursor=true;
    static int mous_x=-1,mous_y=-1;    
    static int tick=0;
    static bool dirtydraw=false;
    static int lvl_w,lvl_h,lvl=0;
    static char[] board;
    static int full=0,moves,emptys;
    static string lev_file,rec_file;
    static DateTime lev_filetime,rec_filetime;
    static List<string> level;
    static SortedDictionary<int,int> records=new SortedDictionary<int,int>();
    static bool start,stop;
    static int start2,players;
    static string help;
    Graphics gr2;
    Bitmap bm2;
    string kb_lvl="";
    int anim=0;
    bool mirror_x,mirror_y;
    string final_text;
    Brush final_brush;
    int starticks,ticks;   
    bool show_time,show_clock; 
		bool single,dual,solo,auto=true;

		static Form1() {
		  string path=imagepath+@"\";
      img_wall=Image.FromFile(path+"wall.png");
      img_full=Image.FromFile(path+"full.png");
			img_empty=Image.FromFile(path+"empty.png");
			img_me=Image.FromFile(path+"me.png");
			string fn=path+"me2.png";
			if(File.Exists(fn)) img_me2=Image.FromFile(fn);else img_me2=img_me;
			img_he=Image.FromFile(path+"he.png");
			fn=path+"he2.png";
			if(File.Exists(fn)) img_he2=Image.FromFile(fn);else img_he2=img_he;
		}
    
    
    Image CharImage(char ch) {
      if(Level.IsWall(ch)) return img_wall;
      if(Level.IsFull(ch)) return img_full;
      return img_empty;
    }
    char BoardXY(int x,int y) {
      x=(x+lvl_w)%lvl_w;
      y=(y+lvl_h)%lvl_h;
      return board[y*lvl_w+x];
    }
    void Clip2Clip() {
       try { 
        Image ci=Clipboard.GetImage();
        Bitmap bm=new Bitmap(ci);
        int sx=0,sy=0,dx=0,dy=0,argb=Color.Black.ToArgb();        
        for(sy=0;sy<bm.Height;sy++) {
          for(sx=0;sx<bm.Width;sx++) {
            if(argb!=bm.GetPixel(sx,sy).ToArgb())
              goto corner1;
          }
        }
       corner1: 
        for(dy=bm.Height-1;dy>sy;dy--) {
          for(dx=bm.Width-1;dx>sx;dx--) {
            if(argb!=bm.GetPixel(dx,dy).ToArgb())
              goto corner2;
          }
        }
       corner2:
        dx=(dx-sx+4)/16;dy=(dy-sy+4)/11;
        string ilvl="";
        for(int y=0;y<11;y++) {          
          for(int x=0;x<16;x++) {
            Color c=bm.GetPixel(sx+dx*x+7*dx/16,sy+dy*y+7*dy/16);
            char ch=c.R>c.G&&c.R>c.B?'W':c.R==c.G&&c.R==255?'1':c.B>c.R&&c.B>c.G?'!':'.';
            ilvl+=ch;
          }
          ilvl+="\r\n";
        }          
        bm.Dispose();
        ci.Dispose();
        Clipboard.SetText(ilvl);
        level[lvl]=ilvl;
        LoadLevel(lvl);
       } catch {}    
    }
    void Status(int level,int moves,int emptys) {
      int rec;
      bool isrec=records.TryGetValue(level+1,out rec);
      Text="KULILI - level "+(level+1)+(emptys<1?"":"  moves:"+emptys)+(isrec?" record:"+rec:"")+(moves<=emptys?"":" points:"+(moves-emptys));
    }
    
    void LoadLevel(int l) {    
      if(lev_filetime!=File.GetLastWriteTimeUtc(lev_file)) {
        level=Level.Load(lev_file,out lev_filetime);
        if(l>=level.Count) l=lvl=level.Count-1;
      }
      final_text=null;
      full=Level.Init(level[l],ref lvl_w,ref lvl_h,ref board
        ,my,my2,he,he2,single,solo);
      if(mirror_x||mirror_y) {
        if(mirror_y) {
         for(int y=0,y2=lvl_h-1;y<y2;y++,y2--) {
          for(int x=0;x<lvl_w;x++) {
            char ch=board[y*lvl_w+x];
            board[y*lvl_w+x]=board[y2*lvl_w+x];
            board[y2*lvl_w+x]=ch;
          }
         }
         my.y=lvl_h-1-my.y;
				 if(my2.y>=0) my2.y=lvl_h-1-my2.y;
         he.y=lvl_h-1-he.y;
         if(he2.y>=0) he2.y=lvl_h-1-he2.y;
        }
        if(mirror_x) {
         for(int x=0,x2=lvl_w-1;x<x2;x++,x2--) {
          for(int y=0;y<lvl_h;y++) {
            char ch=board[y*lvl_w+x];
            board[y*lvl_w+x]=board[y*lvl_w+x2];
            board[y*lvl_w+x2]=ch;
          }
         }
         my.x=lvl_w-1-my.x;
				 if(my2.x>=0) my2.x=lvl_w-1-my2.x;
         he.x=lvl_w-1-he.x;
         if(he2.x>=0) he2.x=lvl_w-1-he2.x;
        }
      }
      he.x2=he.x;he.y2=he.y;
      he2.x2=he2.x;he2.y2=he2.y;
      my.x2=my.x;my.y2=my.y;
      he.my=my.x<0?my2:my;he2.my=my2.x<0?my:my2;
      my.emptys=my.moves=my2.emptys=my2.moves=0;
      emptys=moves=0;
      start=!(stop=false);
      start2=0;
			players=(my.x>=0?1:0)+(my2.x>=0?1:0);
      my.go_clear();my2.go_clear();      
      Status(l,moves,emptys);
      Graphics gr=this.CreateGraphics();      
      bm2=new Bitmap(lvl_w*img_wall.Width,lvl_h*img_wall.Height,gr);//System.Drawing.Imaging.PixelFormat.Format32bppRgb);      
      if(gr2!=null) gr2.Dispose();
      gr2=Graphics.FromImage(bm2);
      gr.Dispose();
      UpdateSize();
      //DrawBack();
    }
    void Mirror(bool dox,bool doy) {
      int x,x2,y,y2;
      char r;
      if(dox) {
        mirror_x^=true;
        for(y=0;y<lvl_h;y++)
          for(x=0,x2=lvl_w-1;x<x2;x++,x2--) {
            r=board[y*lvl_w+x];
            board[y*lvl_w+x]=board[y*lvl_w+x2];
            board[y*lvl_w+x2]=r;
          }
        my.x2=my.x=lvl_w-my.x-1;
				if(my2.x>=0) my2.x2=my2.x=lvl_w-my2.x-1;
        he.x2=he.x=lvl_w-he.x-1;
				if(he2.x>=0) he2.x2=he2.x=lvl_w-he2.x-1;
      }
      if(doy) {
        mirror_y^=true;
        for(x=0;x<lvl_w;x++)
          for(y=0,y2=lvl_h-1;y<y2;y++,y2--) {
            r=board[y*lvl_w+x];
            board[y*lvl_w+x]=board[y2*lvl_w+x];
            board[y2*lvl_w+x]=r;
          }
        my.y2=my.y=lvl_h-my.y-1;
				if(my2.x>=0) my2.y2=my2.y=lvl_h-my2.y-1;
        he.y2=he.y=lvl_h-he.y-1;
				if(he2.x>=0) he2.y2=he2.y=lvl_h-he2.y-1;
      }
      DrawBack();
    }
    static void LoadRecords(string filename) {
      TextReader tr=null;      
     try {
      DateTime filetime=File.GetLastWriteTimeUtc(filename);
      if(filetime==rec_filetime||filetime.Year==1601) return;
      rec_filetime=filetime;
      tr=new StreamReader(filename,System.Text.Encoding.Default);
      string line;      
      while(null!=(line=tr.ReadLine())) {
        string[] sa=line.Split(' ','\t');
        int lvl=-1,sai;
        foreach(string sax in sa) {
          if(!int.TryParse(sax,out sai)) continue;
          if(lvl<1) lvl=sai;
          else if(sai>=0) {
            int act;
            if(!records.TryGetValue(lvl,out act)||act<1||act>sai)
              records[lvl]=sai;
            break;
          }
        }
      }
     } catch {
     } finally {
      if(tr!=null) tr.Close(); 
     }
    }
    static void SaveRecords(string filename) {
      TextWriter tw=null;
     try {
      tw=new StreamWriter(filename,false);
      foreach(KeyValuePair<int,int> kv in records)
        tw.WriteLine(""+kv.Key+" "+kv.Value);
     } catch {
     } finally {
      if(tw!=null) {
        tw.Close();
        rec_filetime=File.GetLastWriteTimeUtc(filename);
      }
     }
    }
    void Help() {
      MessageBox.Show(@"-s : single, without second player
 -d : dual both balls with one keys
 -o : no enemy
 -a : autopilot off","Help",MessageBoxButtons.OK);
    }
    public Form1(string[] args) {
		  int a=0;
      lev_file="levels.txt";
			while(a<args.Length&&args[a][0]=='-') {
			  string arg=args[a++];
				if(arg=="-") break;
			  switch(arg) {
				 case "-s":single=true;break;
         case "-d":dual=true;break;
         case "-a":auto=false;break;
         case "-o":solo=true;break;
         case "-?":
         case "-h":Help();break;
				}
			}
      if(args.Length>a) lev_file=args[a];
      string ext=Path.GetExtension(lev_file);
      string rec="_rec"+(solo?"_solo":"")+(dual?"_dual":"");
      rec_file=string.IsNullOrEmpty(ext)?lev_file+rec:lev_file.Substring(0,lev_file.Length-ext.Length)+rec+ext;
      level=Level.Load(lev_file,out lev_filetime);
      LoadRecords(rec_file);
      LoadLevel(lvl);
      InitializeComponent();
/*      
     try {
      if(File.Exists("move.wav"))
        move_sound=new SoundPlayer("move.wav");
      if(File.Exists("full.wav"))
        full_sound=new SoundPlayer("full.wav");
     } catch {} */
      timer1.Interval=timer_interval;
      if(ww*lvl_w>0) {
        this.ClientSize=new Size(ww*lvl_w,hh*lvl_h);
      }
    }

    protected override void OnShown(EventArgs e) {
      base.OnShown(e);
      UpdateSize();
    }
    protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
      //Text=""+keyData;
      if(stop&&keyData>=Keys.NumPad0&&keyData<=Keys.NumPad9)
        keyData-=Keys.NumPad0-Keys.D0;
      if(keyData>=Keys.D0&&keyData<=Keys.D9)
        kb_lvl+=(char)keyData;
      if(kb_lvl.Length==2||kb_lvl.Length>0&&keyData==Keys.Enter) {
        lvl=int.Parse(kb_lvl)-1;
        kb_lvl="";
        if(lvl<0) lvl=0;
        if(lvl>level.Count-1) lvl=level.Count-1;
        LoadLevel(lvl);
        return true;
      }
      bool control=0!=(keyData&Keys.Control),shift=0!=(keyData&Keys.Shift),alt=0!=(keyData&Keys.Alt);
      if(control) keyData&=~Keys.Control;
      if(shift) keyData&=~Keys.Shift;
      if(alt) keyData&=~Keys.Alt;
      
      switch(keyData) {
       case Keys.Escape:
         if(start||stop) Close();
         else LoadLevel(lvl);
         return true;
       case Keys.L:
         LoadLevel(lvl);
         return true;
       case Keys.Back:
        if(start&&lvl>0) lvl--;       
        LoadLevel(lvl);return true;
       case Keys.Space:
       case Keys.Enter:         
         if(alt|control) ChangeMaxi();
         else if(start||stop) {
           if(start) {
             if(shift) {shift=false;goto leveldown;} else goto levelup;
           }
           LoadLevel(lvl);
         }
         return true;
       case Keys.PageDown:
       leveldown: 
        if((start||stop)) {
          int lvl2=lvl-(control?10:shift?5:1);
          if(lvl2<0) lvl2=0;
          if(lvl2!=lvl||stop) {
            LoadLevel(lvl=lvl2);
          }
        }
        return true;        
       case Keys.PageUp:
       levelup: 
        if((start||stop)) {
          int lvl2=lvl+(control?10:shift?5:1);
          if(lvl2>=level.Count) lvl2=level.Count-1;
          if(lvl2!=lvl||stop) {
            LoadLevel(lvl=lvl2);
          }
        }
        return true;
       case Keys.F1: {
         if(help==null) 
          try { help=File.ReadAllText(Path.Combine(imagepath,"readme.txt"),Encoding.UTF8);
          } catch {
           MessageBox.Show("Unable to read help file 'readme.txt'","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
           break;
          }
         MessageBox.Show(help,"Kulili help",MessageBoxButtons.OK,MessageBoxIcon.Information);
        } break;
       case Keys.F4:
       case Keys.X:
        Mirror(true,false);break;       
       case Keys.F5:
       case Keys.Y:case Keys.Z:
        Mirror(false,true);break;
       case Keys.T:show_time^=true;if(!show_time) dirtydraw=true;else DrawTime();break;
       case Keys.C:show_clock^=true;if(!show_clock) dirtydraw=true;else DrawClock();break;
       case Keys.F10:
        Clip2Clip();
        break;
       case Keys.F:
       case Keys.F11:       
        ChangeMaxi();        
        return true;
      }
      return base.ProcessCmdKey(ref msg, keyData);
    }
    protected override void OnPaintBackground(PaintEventArgs e) {
      e.Graphics.FillRectangle(Brushes.Black,e.ClipRectangle);
    }
    protected override void OnPaint(PaintEventArgs e) {
      dirtydraw=true;
    }
    protected override void OnResize(EventArgs e) {
      base.OnResize(e);
      UpdateSize();      
    }
    void UpdateSize() {
      int cw=ClientRectangle.Width,ch=ClientRectangle.Height;
      int size=cw/lvl_w/img_wall.Width,sizey=ch/lvl_h/img_wall.Height;
      if(sizey<size) size=sizey;
      if(size<=0) size=1;
      scale=size;
      ww=size*img_wall.Width;
      hh=size*img_wall.Height;
      sx=(cw-lvl_w*ww)/2;
      sy=(ch-lvl_h*hh)/2;
      dirtydraw=true;
    }

    static void Draw(Graphics gr,Image img,int x,int y) { 
      DrawXY(gr,img,x*img_wall.Width,y*img_wall.Height);
    }
    static void DrawXY(Graphics gr,Image img,int x,int y) {
      gr.DrawImageUnscaled(img,x,y);
    }
    protected void DrawBack() {
      gr2.FillRectangle(Brushes.Black,0,0,bm2.Width,bm2.Height);
      int w2=img_wall.Width,h2=img_wall.Height;
      for(int y=0;y<lvl_h;y++)
        for(int x=0;x<lvl_w;x++) {
          Image img=CharImage(board[y*lvl_w+x]);
          if(img==img_full) gr2.DrawImageUnscaled(img_empty,x*w2,y*h2);
          gr2.DrawImageUnscaled(img,x*w2,y*h2);
        }
      if(my.x>=0) gr2.DrawImageUnscaled(img_me,my.x*w2,my.y*h2);
      if(my2.x>=0) gr2.DrawImageUnscaled(img_me2,my2.x*w2,my2.y*h2);
      if(he.x>=0) gr2.DrawImageUnscaled(img_he,he.x*w2,he.y*h2);
      if(he2.x>=0) gr2.DrawImageUnscaled(img_he2,he2.x*w2,he2.y*h2);
      DrawFinalText();
      Graphics gr=this.CreateGraphics();      
      gr.DrawImage(bm2,sx,sy,lvl_w*ww,lvl_h*hh);  
      gr.Dispose();
    }

    static Font fnt=new Font("Tahoma",10,FontStyle.Bold);
    static Font fnt8=new Font("Tahoma",8,FontStyle.Bold);
    void DrawFinalText() {
      if(string.IsNullOrEmpty(final_text)) return;            
      Font f=fnt;
      SizeF sz=gr2.MeasureString(final_text,f);
      int fw=(int)(sz.Width+0.99),fh=(int)(sz.Height+0.99);
      int rw=lvl_w*img_wall.Width-16;
      int rx=(lvl_w*img_wall.Width-rw)/2;
      int tx=(lvl_w*img_wall.Width-fw)/2;
      int ty=(lvl_h*img_wall.Height-fh)/2;
      if(tx<rx+2) tx=rx+2;
      gr2.FillRectangle(Brushes.Black,rx-2,ty-2,rw+4,fh+4);
      gr2.DrawRectangle(new Pen(Brushes.White,1),rx-2,ty-2,rw+4,fh+4);
      gr2.DrawString(final_text,f,final_brush,tx,ty);    
    }

    void DrawMoves() {
      Graphics gr=this.CreateGraphics();    
      for(int i=0;i<2;i++) {
        My mi=i==0?my:my2;  
        if(mi.x<0) continue;
        string txt=string.Format("{0:000}-{1:000}",mi.emptys,mi.moves-mi.emptys);
        SizeF sz=gr2.MeasureString(txt,fnt8);
        Rectangle src=new Rectangle(i==0?0:img_wall.Width*lvl_w-(int)sz.Width,img_wall.Height*lvl_h-(int)sz.Height,(int)sz.Width,(int)sz.Height);
        gr2.FillRectangle(Brushes.Black,src.Left-1,src.Top-1,src.Right+1,src.Bottom+1);
        gr2.DrawString(txt,fnt8,Brushes.White,src.Left,src.Top);      
        Rectangle dst=new Rectangle(sx+scale*src.Left,sy+scale*src.Top,scale*src.Width,scale*src.Height);
        gr.DrawImage(bm2,dst,src,GraphicsUnit.Pixel);  
      }
      gr.Dispose();
    }    
    void DrawTime() {
      DrawMoves();
      string txt=string.Format("{0:00}:{1:00}.{2}",ticks/60000,ticks/1000%60,ticks/100%10);
      SizeF sz=gr2.MeasureString(txt,fnt8);
      Rectangle src=new Rectangle(0,0,(int)sz.Width,(int)sz.Height);
      gr2.FillRectangle(Brushes.Black,src.Left-1,src.Top-1,src.Right+1,src.Bottom+1);
      gr2.DrawString(txt,fnt8,Brushes.White,src.Left,src.Top);      
      Graphics gr=this.CreateGraphics();
      Rectangle dst=new Rectangle(sx+scale*src.Left,sy+scale*src.Top,scale*src.Width,scale*src.Height);
      gr.DrawImage(bm2,dst,src,GraphicsUnit.Pixel);  
      gr.Dispose();
    }
    void DrawClock() {
      DateTime t=DateTime.Now;
      string txt=string.Format("{0:00}:{1:00}",t.Hour,t.Minute);
      SizeF sz=gr2.MeasureString(txt,fnt8);
      Rectangle src=new Rectangle(img_wall.Width*lvl_w-(int)sz.Width,0,(int)sz.Width,(int)sz.Height);
      gr2.FillRectangle(Brushes.Black,src.Left-1,src.Top-1,src.Right+1,src.Bottom+1);
      gr2.DrawString(txt,fnt8,Brushes.White,src.Left,src.Top);      
      Graphics gr=this.CreateGraphics();
      Rectangle dst=new Rectangle(sx+scale*src.Left,sy+scale*src.Top,scale*src.Width,scale*src.Height);
      gr.DrawImage(bm2,dst,src,GraphicsUnit.Pixel);  
      gr.Dispose();
    }    
      
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    public static extern int MapVirtualKeyEx(int uCode, int uMapType, IntPtr dwhkl);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    public static extern IntPtr GetKeyboardLayout(int Thread);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    public static extern IntPtr LoadKeyboardLayout(string pwszKLID, int Flags);
    public static IntPtr HKL=LoadKeyboardLayout("00000409",0);


    void Key1(bool up,Dir d,My m) {
      int dx=d==Dir.Left?-1:d==Dir.Right?1:0,dy=d==Dir.Up?-1:d==Dir.Down?1:0;
      if(up) m.dir_remove(d); else { 
        m.dir_add(d);m.dir_remove(Back(d));
        try_key(m,dx,dy);AfterKeyDown(m);
      }
    }
    void Key2(bool up,Dir d,My m) {
      Key1(up,d,m);
      if(dual&&my2.x>=0) Key1(up,d,m==my2?my:my2);	
    }
    
    protected override void OnKeyUp(KeyEventArgs e) { OnKey(true,e);}
		protected override void OnKeyDown(KeyEventArgs e) { OnKey(false,e);}
		protected void OnKey(bool up,KeyEventArgs e) {
      int scan=MapVirtualKeyEx((int)e.KeyCode,4,GetKeyboardLayout(0));
      Keys k=(Keys)MapVirtualKeyEx(scan,3,HKL);
		  My m2=my2.x>=0?my2:my;
      switch(e.KeyCode) {
			 case Keys.W:
       case Keys.Q:
        Key2(up,Dir.Up,my);
			  //if(up) my.dir_remove(Direction.Up); else { my.dir_add(Direction.Up);try_key(my,0,-1);AfterKeyDown(my);}
				break;
			 case Keys.S:
       case Keys.A:
        Key2(up,Dir.Down,my);
			  //if(up) my.dir_remove(Direction.Down); else { my.dir_add(Direction.Down);try_key(my,0,1);AfterKeyDown(my);}
				break;
			 case Keys.U:
			 case Keys.E:
       case Keys.O:
        Key2(up,Dir.Left,my);
			  //if(up) my.dir_remove(Direction.Left); else { my.dir_add(Direction.Left);try_key(my,-1,0);AfterKeyDown(my);}
				break;
			 case Keys.I:
			 case Keys.R:
       case Keys.P:
        Key2(up,Dir.Right,my);
        //if(up) my.dir_remove(Direction.Right); else { my.dir_add(Direction.Right);try_key(my,1,0);AfterKeyDown(my);}
				break;
       case Keys.Up:
			 case Keys.NumPad7:
       case Keys.NumPad1:
        Key2(up,Dir.Up,m2);
			  //if(up) m2.dir_remove(Direction.Up); else { m2.dir_add(Direction.Up);try_key(m2,0,-1);AfterKeyDown(m2);}
				break;
       case Keys.Down:
       case Keys.NumPad4:
       case Keys.NumPad0:
        Key2(up,Dir.Down,m2);
			  //if(up) m2.dir_remove(Direction.Down); else { m2.dir_add(Direction.Down);try_key(m2,0,1);AfterKeyDown(m2);}
				break;
       case Keys.Left:
       case Keys.NumPad8:
       case Keys.NumPad5:
       case Keys.NumPad2:
        Key2(up,Dir.Left,m2);
			  //if(up) m2.dir_remove(Direction.Left); else { m2.dir_add(Direction.Left);try_key(m2,-1,0);AfterKeyDown(m2);}
				break;
       case Keys.Right:
       case Keys.NumPad9:
       case Keys.NumPad6:
       case Keys.NumPad3:
        Key2(up,Dir.Right,m2);
			  //if(up) m2.dir_remove(Direction.Right); else { m2.dir_add(Direction.Right);try_key(m2,1,0);AfterKeyDown(m2);}
				break;
      }
    }
    void AfterKeyDown(My mx) {
      if(WindowState==FormWindowState.Maximized&&mx==my) ShowCursor(false);
    }
    
    void ShowCursor(bool show) {
      if(cursor==show) return;      
      cursor=show;
      if(cursor) Cursor.Show();
      else Cursor.Hide();
    }
    void OnStart() {
      start=false;
      starticks=Environment.TickCount;      
    }
    void OnStart2() {
      if(my2.x<0||my.x<0) OnStart();
      else if(start2==0) start2=1;
    }
    protected override void OnMouseDown(MouseEventArgs e) {
      base.OnMouseDown(e);
      mous=0!=(e.Button&MouseButtons.Left);
      if(mous) {
        mous_x=(e.X-sx)/ww;mous_y=(e.Y-sy)/hh;
        bool maxi=WindowState==FormWindowState.Maximized;
        if(maxi) ShowCursor(true);
        if(stop) {
          if(full==0&&lvl<level.Count-1) lvl++;
          LoadLevel(lvl);
        } else if(start) OnStart2();
      }
    }
    protected override void OnMouseUp(MouseEventArgs e) {
      base.OnMouseUp(e);      
      mous=0!=(e.Button&MouseButtons.Left);
    }
    protected override void OnMouseMove(MouseEventArgs e){
      base.OnMouseMove(e);
      mous=0!=(e.Button&MouseButtons.Left);
      if(mous) {
        mous_x=(e.X-sx)/ww;mous_y=(e.Y-sy)/hh;
      }
    }

    protected override void OnMouseDoubleClick(MouseEventArgs e) {
      if(mous_x==my.x&&mous_y==my.y)
        ChangeMaxi();
    }
    
    void ChangeMaxi() {
      bool maxi=!(WindowState==FormWindowState.Maximized);
      this.FormBorderStyle=maxi?FormBorderStyle.None:FormBorderStyle.Sizable;
      WindowState=maxi?FormWindowState.Maximized:FormWindowState.Normal;    
      ShowCursor(!maxi);
    }
    
    void try_key(My mi,int dx,int dy) {
      if(start) OnStart2();
      if(mi.go_left&&dx>0) return;
      if(mi.go_right&&dx<0) return;
      if(Level.IsWall(BoardXY(mi.x+dx,mi.y+dy))) return;
      mi.go_clear();
      if(dx<0) mi.go_left=true;else if(dx>0) mi.go_right=true;
      if(dy<0) mi.go_up=true;else if(dy>0) mi.go_down=true;
    }    
    
    My Target(He p) {
      if(my.x<0) return my2;else if(my2.x<0) return my;
      int d=Math.Abs(p.x-my.x)+Math.Abs(p.y-my.y);
      int d2=Math.Abs(p.x-my2.x)+Math.Abs(p.y-my2.y);
      return d==d2?p.my:d<d2?my:my2;
    }

    private void timer1_Tick(object sender, EventArgs e) {
      tick++;                  
      if(dirtydraw) {
        DrawBack();
        if(show_time) DrawTime();
        if(show_clock) DrawClock();
        dirtydraw=false;
      }
      if(show_clock&&0==(tick%10)) DrawClock();
      if(start&&(my.go_left||my.go_right||my.go_up||my.go_down)) OnStart2();
      if(start&&my2.x>=0&&start2>0) {
        start2++;
        if(start2>=anim_max) OnStart();
      }
      if(!start&&!stop) {
        ticks=Environment.TickCount-starticks;
        if(show_time) DrawTime();
      }
      if(anim==0&&!start&&!stop) {
        if(!my.go_left&&!my.go_right&&!my.go_up&&!my.go_down) my.go_copy();
        if(!my2.go_left&&!my2.go_right&&!my2.go_up&&!my2.go_down) my2.go_copy();
        my.x2=my.x;my.y2=my.y;my2.x2=my2.x;my2.y2=my2.y;
        int x2,y2,im=2;
        for(int i=0;i<im;i++) {
          My mi=i==1?my2:my,mj=i==1?my:my2;
          if(mi.x<0) continue;
          x2=mi.x;y2=mi.y;
          Dir skip=Dir.None;          
          bool found=false;
          List<Dir> kb2=mi.kb;
          if(mous&&(mi==my||dual)&&kb2.Count==0) {
            kb2=new List<Dir>();
            int msx=mous_x,msy=mous_y;
            if(msx<mi.x) kb2.Add(Dir.Left); else if(msx>mi.x) kb2.Add(Dir.Right);
            if(msy<mi.y) kb2.Add(Dir.Up); else if(msy>mi.y) kb2.Add(Dir.Down);
          }
          foreach(Dir dir in kb2) {
            if(0!=(dir&skip)) continue;
            x2=(mi.x+(dir==Dir.Left?-1:0)+(dir==Dir.Right?1:0)+lvl_w)%lvl_w;
            y2=(mi.y+(dir==Dir.Up?-1:0)+(dir==Dir.Down?1:0)+lvl_h)%lvl_h;
            found=(x2!=mi.x||y2!=mi.y)&&!Level.IsWall(BoardXY(x2,y2))&&(x2!=he.x||y2!=he.y)&&(x2!=he2.x||y2!=he2.y);
            if(found) {
               found&=(x2!=mj.x||y2!=mj.y);
               if(!found&&i==0) {im=3;goto ni;}
            }
            if(found)
              break;
            skip|=Back(dir);
          }
          // autopilot
          if(!found&&mi.kb.Count==0&&auto)
            for(int i2=0;i2<2;i2++) {
              He hi=i2==1?he2:he,hj=i2==1?he:he2;
              if(Math.Abs(mi.x-hi.x)+Math.Abs(mi.y-hi.y)!=1||Math.Abs(mi.x-hj.x)+Math.Abs(mi.y-hj.y)==1) continue;
              x2=mi.x;y2=mi.y;
              bool wl=Level.IsWall(BoardXY(mi.x-1,mi.y));
              bool wr=Level.IsWall(BoardXY(mi.x+1,mi.y));
              bool wu=Level.IsWall(BoardXY(mi.x,mi.y-1));
              bool wd=Level.IsWall(BoardXY(mi.x,mi.y+1));
              if((wl?1:0)+(wr?1:0)+(wu?1:0)+(wd?1:0)==2) {          
               if(!wl&&(mi.x-1!=hi.x||mi.y!=hi.y)) x2=mi.x-1;
               if(!wr&&(mi.x+1!=hi.x||mi.y!=hi.y)) x2=mi.x+1;
               if(!wu&&(mi.x!=hi.x||mi.y-1!=hi.y)) y2=mi.y-1;
               if(!wd&&(mi.x!=hi.x||mi.y+1!=hi.y)) y2=mi.y+1;
               found=true;
              }
            }
          if(found) {
            mi.x=x2;mi.y=y2;
            int bidx=y2*lvl_w+x2;
            bool isfull;
            if((isfull=Level.IsFull(board[bidx]))) {
              board[bidx]=Level.Empty;
              full--;
              if(full<1) stop=true;
            } else {
              mi.emptys++;
              emptys++;
            }
            mi.moves++;
            moves++;
          }
         ni:;
        }
        if(my.x!=my.x2||my.y!=my.y2||my2.x!=my2.x2||my2.y!=my2.y2) {
          anim=anim_max;
          Status(lvl,moves,emptys);          
/*          SoundPlayer sound=isfull?full_sound:move_sound;
          if(sound!=null) {
            sound.Play();
          }*/
        }
        he.x2=he.x;he.y2=he.y;
        he2.x2=he2.x;he2.y2=he2.y;
        for(int i=0;i<3;i++) {
          He hi=i==1?he2:he,hj=i==1?he:he2;
          if(hi.x<0||hi.x!=hi.x2) continue;
          My mi=hi.my=Target(hi);
          x2=(hi.x<mi.x?1:hi.x>mi.x?-1:0);
          if(x2!=0&&!Level.IsWall(BoardXY(hi.x+x2,hi.y))&&!(hi.x+x2==hj.x&&hi.y==hj.y)) {
            hi.x+=x2;
          }
        }
        for(int i=0;i<3;i++) {
          He hi=i==1?he2:he,hj=i==1?he:he2;
          if(hi.x<0||hi.x!=hi.x2||hi.y!=hi.y2) continue;
          My mi=hi.my;//=Target(hi);
          y2=(hi.y<mi.y?1:hi.y>mi.y?-1:0);
          if(y2!=0&&!Level.IsWall(BoardXY(hi.x,hi.y+y2))&&!(hi.y+y2==hj.y&&hi.x==hj.x)) {
            hi.y+=y2;
          }
        }
        for(int i=0;i<2;i++) {
          He hi=i==1?he2:he,hj=i==1?he:he2;
          if(hi.x<0||hi.x!=hi.x2||hi.y!=hi.y2) continue;
          My mi=hi.my;//Target(hi);
          x2=(hi.x<mi.x?1:hi.x>mi.x?-1:0);
          if(x2!=0&&!Level.IsWall(BoardXY(hi.x+x2,hi.y))&&!(hi.x+x2==hj.x&&hi.y==hj.y)) {
            hi.x+=x2;
          }
        }
        if(he.x>=0&&(he.x!=he.x2||he.y!=he.y2)||he2.x>=0&&(he2.x!=he2.x2||he2.y!=he2.y2))
          anim=anim_max+1;
      }
      if(anim>0) { //&&my.x2!=my.x||my.y2!=my.y||he.x2!=he.x||he.y2!=he.y) {
        Draw(gr2,img_empty,my.x,my.y);
        Draw(gr2,img_empty,my.x2,my.y2);
        if(my2.x>=0) {
          Draw(gr2,img_empty,my2.x,my2.y);
          Draw(gr2,img_empty,my2.x2,my2.y2);
        }
        for(int i=0;i<2;i++) {
          He hi=i==0?he:he2;
          if(hi.x<0||hi.x==hi.x2&&hi.y==hi.y2) continue;
          Draw(gr2,img_empty,hi.x,hi.y);
          if(Level.IsFull(BoardXY(hi.x,hi.y)))
            Draw(gr2,img_full,hi.x,hi.y);
          Draw(gr2,img_empty,hi.x2,hi.y2);
          if(Level.IsFull(BoardXY(hi.x2,hi.y2)))
            Draw(gr2,img_full,hi.x2,hi.y2);
        }        
        int r=(anim_max+1-anim),r2=anim_max-r;
        DrawXY(gr2,img_me,(r*my.x+r2*my.x2)*img_wall.Width/anim_max,(r*my.y+r2*my.y2)*img_wall.Height/anim_max);
        if(my2.x>=0) DrawXY(gr2,img_me2,(r*my2.x+r2*my2.x2)*img_wall.Width/anim_max,(r*my2.y+r2*my2.y2)*img_wall.Height/anim_max);
        for(int i=0;i<2;i++) {
          He hi=i==0?he:he2;
          if(hi.x<0||hi.x==hi.x2&&hi.y==hi.y2) continue;
          DrawXY(gr2,i==0?img_he:img_he2,(r*hi.x+r2*hi.x2)*img_wall.Width/anim_max,(r*hi.y+r2*hi.y2)*img_wall.Height/anim_max);
        }
        Graphics gr=this.CreateGraphics();        
        Rectangle src=new Rectangle(),dst=new Rectangle();
        int w2=img_wall.Width,h2=img_wall.Height;
        for(int i=0;i<2;i++) {
          My mi=i==0?my:my2;
          src.X=mi.x*w2-1;src.Y=mi.y*h2-1;src.Width=w2+2;src.Height=h2+2;
          dst.X=sx+mi.x*ww-scale;dst.Y=sy+mi.y*hh-scale;dst.Width=ww+2*scale;dst.Height=hh+2*scale;
          gr.DrawImage(bm2,dst,src,GraphicsUnit.Pixel);  
          src.X=mi.x2*w2-1;src.Y=mi.y2*h2-1;
          dst.X=sx+mi.x2*ww-scale;dst.Y=sy+mi.y2*hh-scale;
          gr.DrawImage(bm2,dst,src,GraphicsUnit.Pixel);
        }


        for(int i=0;i<2;i++) {
          He hi=i==0?he:he2;
          if(hi.x<0||hi.x==hi.x2&&hi.y==hi.y2) continue;                
          src.X=hi.x*w2-1;src.Y=hi.y*h2-1;
          dst.X=sx+hi.x*ww-scale;dst.Y=sy+hi.y*hh-scale;
          gr.DrawImage(bm2,dst,src,GraphicsUnit.Pixel);  
          src.X=hi.x2*w2-1;src.Y=hi.y2*h2-1;
          dst.X=sx+hi.x2*ww-scale;dst.Y=sy+hi.y2*hh-scale;
          gr.DrawImage(bm2,dst,src,GraphicsUnit.Pixel);  
        }

        if(r2==0) {
          my.x2=my.x;my.y2=my.y;
          my2.x2=my2.x;my2.y2=my2.y;
          he.x2=he.x;he.y2=he.y;
          he2.x2=he2.x;he2.y2=he2.y;
					bool catch1=my.x>=0&&(he.x==my.x&&he.y==my.y||he2.x==my.x&&he2.y==my.y);
					bool catch2=my2.x>=0&&(he.x==my2.x&&he.y==my2.y||he2.x==my2.x&&he2.y==my2.y);
					if(catch1&&!catch2&&my2.x>=0) { my.x=-1;catch1=false;}
					if(catch2&&!catch1&&my.x>=0) {my2.x=-1;catch2=false;}
          if(catch1||catch2) {
            stop=true;
            final_text="Level "+(lvl+1)+" failed with "+emptys+" moves !";
            final_brush=Brushes.Red;
            DrawFinalText();
            gr.DrawImage(bm2,sx,sy,lvl_w*ww,lvl_h*hh);  
          } else  if(full==0) {
            int rec;
            bool isrec,ismin=false;
            int mode=he.x<0&&he2.x<0?players>1?2:1:0;
            int score=mode==2?my.moves-my.emptys>my2.moves-my2.emptys?my.moves-my.emptys:my2.moves-my2.emptys:mode==1?emptys:emptys;
            if(!records.TryGetValue(lvl+1,out rec)) isrec=true;
            else {              
              isrec=mode==2?score>rec:score<rec;
              ismin=score==rec;
            }
            if(isrec) {
              records[lvl+1]=score;
              LoadRecords(rec_file);
              SaveRecords(rec_file);
            }
            final_text="Level "+(lvl+1)+(isrec?" record":ismin?" best":" done")+" with "+score+(!isrec?"":"("+(score-rec).ToString("+#;-#;")+")")+" moves!";
            final_brush=isrec?Brushes.Yellow:ismin?Brushes.Cyan:Brushes.White;
            DrawFinalText();
            gr.DrawImage(bm2,sx,sy,lvl_w*ww,lvl_h*hh);  
          }
        }
        gr.Dispose();
        anim--;
      }
    }
    
    static Dir Back(Dir dir) {
      return dir==Dir.Left?Dir.Right:dir==Dir.Right?Dir.Left:dir==Dir.Up?Dir.Down:Dir.Up;
    }
  }
  public enum Dir { None,Left=1,Right=2,Up=4,Down=8}  
  public class He {
    public int x,y,x2,y2;
    public My my;
  }
  public class My:He {
    public List<Dir> kb=new List<Dir>();
    public bool go_up,go_down,go_left,go_right;
    public int emptys,moves;

    public void dir_remove(Dir dir) {
      kb.Remove(dir);
    }
    public void dir_add(Dir dir) {
      if(dir==Dir.None) return; 
      kb.Remove(dir);
      kb.Insert(0,dir);
    }
    public void go_clear() {
      go_up=go_down=go_left=go_right=false;
    }
    public void go_copy() {
      go_clear();
      foreach(Dir d in kb) {
        if(d==Dir.Left) go_left=true;
        if(d==Dir.Right) go_right=true;
        if(d==Dir.Up) go_up=true;
        if(d==Dir.Down) go_down=true;
      }
    }

		public override string ToString() { return "("+x+","+y+","+emptys+")"; }
	}

  
  public static class Level {
    public const char HeFull='!',HeEmpty='?';
    public const char He2Full='$',He2Empty='#';
    public const char Full='.';
    public const char Empty=' ';
    
    public static List<string> Load(string filename,out DateTime filetime) {
      List<string> level=new List<string>();
      TextReader tr=null;
     try {
      tr=new StreamReader(filename,System.Text.Encoding.Default);      
      string line,lvl=null;      
      while(null!=(line=tr.ReadLine())) {
        line=line.TrimEnd();
        bool empty=(line==""||0<=";-+".IndexOf(line[0]));
        if(!empty) lvl+=(lvl==null?"":"\n")+line;
        else if(lvl!=null) {
          level.Add(lvl);
          lvl=null;
        }
      }
      if(lvl!=null) level.Add(lvl);
      filetime=File.GetLastWriteTimeUtc(filename);      
     } catch {
       filetime=DateTime.MinValue;
       level.Add(
@"XXXXXXXXXXXXXXXXXXXXXXXXXXXXX
X1                          X
X    XXXX XXX X    XXXX     X
X    X.   .X. X    X.       X
X    XXX   X  X    XXXX     X
X    X    .X. X    X.       X
X    X    XXX XXXX XXXX     X
X                           X
X XXXX XXX  XXX   XXX  XXX  X
X X.   X  X X  X X   X X  X X
X XXXX XXX. XXX. X   X XXX. X
X X.   X  X X  X X   X X  X X
X XXXX X  X X  X  XXX  X  X X
X!                          X
XXXXXXXXXXXXXXXXXXXXXXXXXXXXX"); 
     } finally {
      if(tr!=null) tr.Close(); 
     }
      return level;     
    }
    public static int Init(string level,ref int lvl_w,ref int lvl_h,ref char[] chars,He my,He my2,He he,He he2,bool single,bool solo) {
      List<string> line=new List<string>();
      lvl_w=0;
      int i2=0;
      for(int i=0;i<=level.Length;i++) {
        if(i==level.Length||level[i]=='\n') {
          string row=level.Substring(i2,i-i2-(i>i2+1&&level[i-1]=='\r'?1:0)).TrimEnd();
          if(row.Length>lvl_w) lvl_w=row.Length;
          line.Add(row);
          i2=i+1;
        }
      }
      lvl_h=line.Count;
      my.x=my.y=my2.x=my2.y=he.x=he.y=he2.x=he2.y=-1;
      int len=lvl_w*lvl_h;
      if(chars==null) chars=new char[len];
      else if(chars.Length<len) Array.Resize(ref chars,len);     
      int full=0;      
      for(int y=0;y<line.Count;y++) {
        string row=line[y];
        int x;
        for(x=0;x<row.Length;x++) {
          char rx=row[x];
          if(IsMy(rx)) { if(rx=='2') if(single) rx=Full;else {my2.x=x;my2.y=y;} else my.x=x;my.y=y;}
          if(IsHe(rx)) { if(!solo) {he.x=x;he.y=y;};if(rx==HeFull) rx=Full;}
          if(IsHe2(rx)) { if(!solo) {he2.x=x;he2.y=y;};if(rx==He2Full) rx=Full;}
          if(IsFull(rx)) full++;          
          chars[y*lvl_w+x]=rx;
        }
        while(x<lvl_w) chars[y*lvl_w+x++]=Level.Empty;
      }
      return full;
    }
    public static bool IsWall(char ch) {
      return Char.IsUpper(ch);
    }
    public static bool IsFull(char ch) {
      return ch=='.'||ch==',';
    }
    public static bool IsMy(char ch) {
      return char.IsDigit(ch);
    }
    public static bool IsHe(char ch) {
      return ch==HeFull||ch==HeEmpty;
    }
    public static bool IsHe2(char ch) {
      return ch==He2Full||ch==He2Empty;
    }    
  }
  
}
