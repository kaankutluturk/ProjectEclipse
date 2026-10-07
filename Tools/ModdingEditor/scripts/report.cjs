// Print a mod's move-change and claim report without VS Code.
// Usage: node scripts/report.cjs <mod folder> [--mods <folder of installed mods>]...
// Exits 1 when a vanilla guard mismatch, unknown move or cross-mod conflict is found.
const fs=require('node:fs/promises'),path=require('node:path');
const project=require('../src/project.cjs'),claims=require('../src/claims.cjs'),report=require('../src/report.cjs');
(async()=>{
    const args=process.argv.slice(2),folders=[];let target;
    for(let i=0;i<args.length;i++){if(args[i]==='--mods')folders.push(args[++i]);else if(!target)target=args[i];else throw new Error(`Unexpected argument: ${args[i]}`);}
    if(!target||folders.some(f=>!f)){console.error('Usage: node scripts/report.cjs <mod folder> [--mods <folder>]...');process.exit(2);}
    const mod=await project.indexMod(path.resolve(target)),others=[];
    for(const folder of folders)for(const entry of await fs.readdir(path.resolve(folder),{withFileTypes:true})){
        const root=path.resolve(folder,entry.name);if(!entry.isDirectory()||root===mod.root)continue;
        try{await fs.access(path.join(root,'mod.toml'));}catch{continue;}
        others.push(await project.indexMod(root));
    }
    process.stdout.write(report.moveReport(mod,project,others)+'\n');
    const problems=[...mod.sources.values()].flatMap(source=>project.analyze(source,mod).issues).filter(i=>i.code.startsWith('native'));
    const conflicts=claims.conflicts([mod,...others]).claims.filter(c=>c.mod===mod);
    process.exit(problems.length||conflicts.length?1:0);
})().catch(e=>{console.error(e.message);process.exit(2);});
