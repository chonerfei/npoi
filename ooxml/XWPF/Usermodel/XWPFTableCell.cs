/* ====================================================================
   Licensed to the Apache Software Foundation (ASF) under one or more
   contributor license agreements.  See the NOTICE file distributed with
   this work for Additional information regarding copyright ownership.
   The ASF licenses this file to You under the Apache License, Version 2.0
   (the "License"); you may not use this file except in compliance with
   the License.  You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
==================================================================== */
namespace NPOI.XWPF.UserModel
{
    using System;
    using NPOI.OpenXmlFormats.Wordprocessing;
    using System.Collections.Generic;
    using System.Text;
    using System.Xml;
    public class XWPFTableCell : IBody
    {
        private CT_Tc ctTc;
        protected List<XWPFParagraph> paragraphs = null;
        protected List<XWPFTable> tables = null;
        protected List<IBodyElement> bodyElements = null;
        protected IBody part;
        private XWPFTableRow tableRow = null;
        /**
         * If a table cell does not include at least one block-level element, then this document shall be considered corrupt
         */
        public XWPFTableCell(CT_Tc cell, XWPFTableRow tableRow, IBody part)
        {
            this.ctTc = cell;
            this.part = part;
            this.tableRow = tableRow;
            // NB: If a table cell does not include at least one block-level element, then this document shall be considered corrupt.
            if(cell.GetPList().Count<1)
                cell.AddNewP();
            bodyElements = new List<IBodyElement>();
            paragraphs = new List<XWPFParagraph>();
            tables = new List<XWPFTable>();
            foreach (object o in ctTc.Items)
            {
                if (o is CT_P)
                {
                    XWPFParagraph p = new XWPFParagraph((CT_P)o, this);
                    paragraphs.Add(p);
                    bodyElements.Add(p);
                }
                if (o is CT_Tbl)
                {
                    XWPFTable t = new XWPFTable((CT_Tbl)o, this);
                    tables.Add(t);
                    bodyElements.Add(t);
                }
            }
        /*
            XmlCursor cursor = ctTc.NewCursor();
            cursor.SelectPath("./*");
            while (cursor.ToNextSelection()) {
                XmlObject o = cursor.Object;
                if (o is CTP) {
                    XWPFParagraph p = new XWPFParagraph((CTP)o, this);
                    paragraphs.Add(p);
                    bodyElements.Add(p);
                }
                if (o is CTTbl) {
                    XWPFTable t = new XWPFTable((CTTbl)o, this);
                    tables.Add(t);
                    bodyElements.Add(t);
                }
            }
            cursor.Dispose();*/
        }



        public CT_Tc GetCTTc()
        {
            return ctTc;
        }

        /**
         * returns an Iterator with paragraphs and tables
         * @see NPOI.XWPF.UserModel.IBody#getBodyElements()
         */
        public IList<IBodyElement> BodyElements
        {
            get
            {
                return bodyElements.AsReadOnly();
            }
        }

        public void SetParagraph(XWPFParagraph p)
        {
            if (ctTc.SizeOfPArray() == 0) {
                ctTc.AddNewP();
            }
            ctTc.SetPArray(0, p.GetCTP());
        }

        /**
         * returns a list of paragraphs
         */
        public IList<XWPFParagraph> Paragraphs
        {
            get
            {
                return paragraphs;
            }
        }

        /**
         * Add a Paragraph to this Table Cell
         * @return The paragraph which was Added
         */
        public XWPFParagraph AddParagraph()
        {
            XWPFParagraph p = new XWPFParagraph(ctTc.AddNewP(), this);
            AddParagraph(p);
            return p;
        }

        /**
         * add a Paragraph to this TableCell
         * @param p the paragaph which has to be Added
         */
        public void AddParagraph(XWPFParagraph p)
        {
            paragraphs.Add(p);
        }

        /**
         * Removes a paragraph of this tablecell
         * @param pos
         */
        public void RemoveParagraph(int pos)
        {
            paragraphs.RemoveAt(pos);
            ctTc.RemoveP(pos);
        }

        /**
         * if there is a corresponding {@link XWPFParagraph} of the parameter ctTable in the paragraphList of this table
         * the method will return this paragraph
         * if there is no corresponding {@link XWPFParagraph} the method will return null 
         * @param p is instance of CTP and is searching for an XWPFParagraph
         * @return null if there is no XWPFParagraph with an corresponding CTPparagraph in the paragraphList of this table
         * 		   XWPFParagraph with the correspondig CTP p
         */
        public XWPFParagraph GetParagraph(CT_P p)
        {
            foreach (XWPFParagraph paragraph in paragraphs) {
                if(p.Equals(paragraph.GetCTP())){
                    return paragraph;
                }
            }
            return null;
        }

        public void SetText(String text)
        {
            CT_P ctP = (ctTc.SizeOfPArray() == 0) ? ctTc.AddNewP() : ctTc.GetPArray(0);
            XWPFParagraph par = new XWPFParagraph(ctP, this);
            par.CreateRun().SetText(text);
        }

        public XWPFTableRow GetTableRow()
        {
            return tableRow;
        }

        /**
         * add a new paragraph at position of the cursor
         * @param cursor
         * @return the inserted paragraph
         */
        public XWPFParagraph insertNewParagraph(/*XmlCursor*/ XmlDocument cursor)
        {
            /*if(!isCursorInTableCell(cursor))
                return null;
    		
            String uri = CTP.type.Name.NamespaceURI;
            String localPart = "p";
            cursor.BeginElement(localPart,uri);
            cursor.ToParent();
            CTP p = (CTP)cursor.Object;
            XWPFParagraph newP = new XWPFParagraph(p, this);
            XmlObject o = null;
            while(!(o is CTP)&&(cursor.ToPrevSibling())){
                o = cursor.Object;
            }
            if((!(o is CTP)) || (CTP)o == p){
                paragraphs.Add(0, newP);
            }
            else{
                int pos = paragraphs.IndexOf(getParagraph((CTP)o))+1;
                paragraphs.Add(pos,newP);
            }
            int i=0;
            cursor.ToCursor(p.NewCursor());
            while(cursor.ToPrevSibling()){
                o =cursor.Object;
                if(o is CTP || o is CTTbl)
                    i++;
            }
            bodyElements.Add(i, newP);
            cursor.ToCursor(p.NewCursor());
            cursor.ToEndToken();
            return newP;*/
            throw new NotImplementedException();
        }

        public XWPFTable insertNewTbl(/*XmlCursor*/ XmlDocument cursor)
        {
            /*if(isCursorInTableCell(cursor)){
                String uri = CTTbl.type.Name.NamespaceURI;
                String localPart = "tbl";
                cursor.BeginElement(localPart,uri);
                cursor.ToParent();
                CTTbl t = (CTTbl)cursor.Object;
                XWPFTable newT = new XWPFTable(t, this);
                cursor.RemoveXmlContents();
                XmlObject o = null;
                while(!(o is CTTbl)&&(cursor.ToPrevSibling())){
                    o = cursor.Object;
                }
                if(!(o is CTTbl)){
                    tables.Add(0, newT);
                }
                else{
                    int pos = tables.IndexOf(getTable((CTTbl)o))+1;
                    tables.Add(pos,newT);
                }
                int i=0;
                cursor = t.NewCursor();
                while(cursor.ToPrevSibling()){
                    o =cursor.Object;
                    if(o is CTP || o is CTTbl)
                        i++;
                }
                bodyElements.Add(i, newT);
                cursor = t.NewCursor();
                cursor.ToEndToken();
                return newT;
            }
            return null;*/
            throw new NotImplementedException();
        }

        /**
         * verifies that cursor is on the right position
         */
        private bool IsCursorInTableCell(/*XmlCursor*/XmlDocument cursor)
        {
            /*XmlCursor verify = cursor.NewCursor();
            verify.ToParent();
            if(verify.Object == this.ctTc){
                return true;
            }
            return false;*/
            throw new NotImplementedException();
        }



        /**
         * @see NPOI.XWPF.UserModel.IBody#getParagraphArray(int)
         */
        public XWPFParagraph GetParagraphArray(int pos)
        {
            if (pos > 0 && pos < paragraphs.Count)
            {
                return paragraphs[(pos)];
            }
            return null;
        }

        /**
         * Get the to which the TableCell belongs
         * 
         * @see NPOI.XWPF.UserModel.IBody#getPart()
         */
        public POIXMLDocumentPart GetPart()
        {
            return tableRow.GetTable().GetPart();
        }


        /** 
         * @see NPOI.XWPF.UserModel.IBody#getPartType()
         */
        public BodyType GetPartType()
        {
            return BodyType.TABLECELL;
        }


        /**
         * Get a table by its CTTbl-Object
         * @see NPOI.XWPF.UserModel.IBody#getTable(org.Openxmlformats.schemas.wordProcessingml.x2006.main.CTTbl)
         */
        public XWPFTable GetTable(CT_Tbl ctTable)
        {
            for(int i=0; i<tables.Count; i++){
                if(GetTables()[(i)].GetCTTbl() == ctTable) return GetTables()[(i)]; 
            }
            return null;
        }


        /** 
         * @see NPOI.XWPF.UserModel.IBody#getTableArray(int)
         */
        public XWPFTable GetTableArray(int pos)
        {
            if (pos > 0 && pos < tables.Count)
            {
                return tables[(pos)];
            }
            return null;
        }


        /** 
         * @see NPOI.XWPF.UserModel.IBody#getTables()
         */
        public IList<XWPFTable> GetTables()
        {
            return tables.AsReadOnly();
        }


        /**
         * inserts an existing XWPFTable to the arrays bodyElements and tables
         * @see NPOI.XWPF.UserModel.IBody#insertTable(int, NPOI.XWPF.UserModel.XWPFTable)
         */
        public void insertTable(int pos, XWPFTable table)
        {
            bodyElements.Insert(pos, table);
            int i;
            for (i = 0; i < ctTc.GetTblList().Count; i++) {
                CT_Tbl tbl = ctTc.GetTblArray(i);
                if(tbl == table.GetCTTbl()){
                    break;
                }
            }
            tables.Insert(i, table);
        }

        public String GetText()
        {
            StringBuilder text = new StringBuilder();
            foreach (XWPFParagraph p in paragraphs)
            {
                text.Append(p.GetText());
            }
            return text.ToString();
        }


        /**
         * Get the TableCell which belongs to the TableCell
         */
        public XWPFTableCell GetTableCell(CT_Tc cell)
        {
            /*XmlCursor cursor = cell.NewCursor();
            cursor.ToParent();
            XmlObject o = cursor.Object;
            if(!(o is CTRow)){
                return null;
            }
            CTRow row = (CTRow)o;
            cursor.ToParent();
            o = cursor.Object;
            cursor.Dispose();
            if(! (o is CTTbl)){
                return null;
            }
            CTTbl tbl = (CTTbl) o;
            XWPFTable table = GetTable(tbl);
            if(table == null){
                return null;
            }
            XWPFTableRow tableRow = table.GetRow(row);
            if(row == null){
                return null;
            }
            return tableRow.GetTableCell(cell);*/
            throw new NotImplementedException();
        }

        public XWPFDocument GetXWPFDocument()
        {
            return part.GetXWPFDocument();
        }
    }// end class
}
