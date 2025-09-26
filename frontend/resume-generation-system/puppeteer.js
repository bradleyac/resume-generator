import puppeteer from 'puppeteer';
import fs from 'fs';
console.error("Butts");
// Launch the browser and open a new blank page.
const browser = await puppeteer.launch();
const page = await browser.newPage();

// Navigate the page to a URL.
await page.goto('http://localhost:5173');

// Set screen size.
// await page.setViewport({ width: '8.5in', height: '11in' });

const pdfBytes = await page.pdf({ printBackground: true });

fs.writeFileSync('./page.pdf', pdfBytes);

await browser.close();