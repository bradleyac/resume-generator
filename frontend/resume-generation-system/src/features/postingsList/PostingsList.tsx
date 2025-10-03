import { useEffect, useState } from "react";
import styles from "./PostingsList.module.css";
import { Modal } from "../modal/Modal";

const BACKEND_URL = import.meta.env.VITE_BACKEND_URL;

type PostingHeader = { id: string, importedAt: string, company: string, title: string, link: string }

export const PostingsList = () => {
  const [postings, setPostings] = useState<PostingHeader[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | undefined>(undefined);
  const [showModal, setShowModal] = useState(false);

  useEffect(() => {
    let ignore = false;

    const fetchItems = async () => {
      try {
        const response = await fetch(`${BACKEND_URL}/api/ListCompletedPostings`);
        if (!response.ok) {
          throw new Error(`HTTP error! status: ${response.status}`);
        }
        const data = await response.json();
        if (!ignore) setPostings(data);
      } catch (error) {
        console.log(error);
        if (error instanceof Error) setError(error.message);
      } finally {
        setLoading(false);
      }
    };

    fetchItems();

    return () => { ignore = true };
  }, []);

  if (loading) {
    return <div>Loading items...</div>;
  }

  if (error) {
    return <div>Error: {error}</div>;
  }

  return (<section>
    <ul className={styles.postingsList}>
      {postings.map(posting => <li key={posting.id}><span>{posting.title} at {posting.company} (imported {new Date(posting.importedAt).toLocaleString()}) </span><a href={`/posting/${posting.id}`}>View Posting</a></li>)}
    </ul>
    <button onClick={() => setShowModal(true)}>Import Job Posting</button>
    <Modal showing={showModal} close={() => setShowModal(false)}>
      <NewPosting closeCallback={() => setShowModal(false)} />
    </Modal>
  </section>);
}

export const NewPosting = ({ closeCallback }: { closeCallback: () => void }) => {
  const submitPosting = async (form: FormData) => {
    await fetch(`${BACKEND_URL}/api/ImportJobPosting`, { method: "POST", body: form });
    closeCallback();
  }

  return (<form action={submitPosting} className={styles.submitPosting}>
    <h1>Submit Job Posting</h1>
    <div className={styles.formControls}>
      <label htmlFor="link">Link: *</label>
      <input type="text" id="link" name="link" required />
      <label htmlFor="company">Company: *</label>
      <input type="text" id="company" name="company" required />
      <label htmlFor="title">Title: *</label>
      <input type="text" id="title" name="title" required />
      <label htmlFor="posting-text" aria-required>Posting Text: *</label>
      <textarea id="posting-text" name="postingText" required />
    </div>
    <input type="submit" />
  </form>)
}