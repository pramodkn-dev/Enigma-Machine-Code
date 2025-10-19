# 🧠 Factory.ai Instruction File
This prompt defines a documentation-only analysis task for the `Enigma_Machine_Code` repository.

---

## 🧭 **Prompt: Retro-Document the Enigma_Machine_Code Repository**

You are analyzing a legacy Java SOAP codebase named **`Enigma_Machine_Code`**.  
Your task is to perform a complete **reverse-engineering and documentation pass** — *without rewriting or migrating the code*.  

All discovered information must be saved locally under a new folder in the repository:
```
/Retro-Documentation/
```

---

### **Your objectives**

1. **Discover and Inventory the Codebase**
   - Scan all source files for:
     - `@WebService`, `@WebMethod`, and related SOAP annotations  
     - Operation names, input/output classes, and checked exceptions  
     - DAO classes and SQL queries (tables, joins, columns)  
     - Configuration and dependencies (`pom.xml`, `web.xml`, Spring context, etc.)  
     - Utility and shared modules (security, logging, caching)  
   - Build a Markdown table mapping:
     ```
     Service | Operation | Input Class | Output Class | Fault | Endpoint | DAO / Tables
     --------|------------|-------------|---------------|--------|-----------|--------------
     ```
   - Include:
     - **Data Access Summary** – which tables each operation uses  
     - **Risk List** – decimal precision, date/time handling, fault mappings, authentication headers, etc.  
     - **Dependency Summary** – libraries, frameworks, and versions used  

   **Output:**  
   Save this as  
   `Retro-Documentation/INVENTORY.md`

---

2. **Capture SOAP Contracts**
   - Locate existing `.wsdl` files or synthesize them from annotations if missing.  
   - For each operation, create example SOAP **request** and **response** XML files.  
   - Store all in:
     ```
     Retro-Documentation/SOAP_Contracts/
     ```
     Example:
     - `service.wsdl`  
     - `GetPriceRequest.xml`  
     - `GetPriceResponse.xml`

---

3. **Generate UML Diagram (Mermaid Format)**
   - Analyze relationships between:
     - Web services → operations → DAOs → database tables  
     - Inter-class and package dependencies  
   - Create a **UML class or component diagram** in **Mermaid** syntax.  
   - Example structure:
     ```mermaid
     classDiagram
         class PriceService {
             +getPrice(req: GetPriceReq): GetPriceRes
         }
         class DAO_Price {
             +queryPrice(id: int): PriceEntity
         }
         PriceService --> DAO_Price
         DAO_Price --> PRICE_TABLE
     ```
   - Save as:  
     `Retro-Documentation/System_UML.mmd`

---

4. **Create an Objectives Summary**
   - Write a concise **Objectives.md** explaining what the codebase does.  
   - Include sections:
     ```
     # Enigma_Machine_Code – Objectives

     ## Purpose
     Summarize in simple language what this system does and why it exists.

     ## Core Functions
     List main services and their responsibilities.

     ## Architecture
     Describe the system’s structure (layers, components, relationships).

     ## Technology Stack
     Mention Java version, frameworks, database, and build tools.

     ## Key Observations
     List noteworthy elements (dependencies, constraints, assumptions).
     ```
   - Save as:  
     `Retro-Documentation/Objectives.md`

---

5. **Final Output Folder Structure**
   ```
   Enigma_Machine_Code/
   ├── src/
   │   └── ...
   └── Retro-Documentation/
       ├── INVENTORY.md
       ├── SOAP_Contracts/
       │   ├── service.wsdl
       │   ├── <operation>Request.xml
       │   ├── <operation>Response.xml
       ├── System_UML.mmd
       └── Objectives.md
   ```

---

### **Key Rules**
- **Do not modify or refactor** any source code.  
- **Do not migrate** to REST or gRPC yet — only document existing SOAP structures.  
- Use **clear Markdown** for all reports.  
- The Mermaid UML must render correctly in Markdown viewers.  
- All output files should be written to `/Retro-Documentation/` in the local workspace.

---

✅ **End of Prompt**
